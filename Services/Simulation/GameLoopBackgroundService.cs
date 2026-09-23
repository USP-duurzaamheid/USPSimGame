using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Services.Costing;

namespace USPSimGame.Services.Simulation;

public class GameLoopBackgroundService : BackgroundService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GameLoopBackgroundService> _logger;

    public GameLoopBackgroundService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<GameLoopBackgroundService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GameLoopBackgroundService: Started monitoring active game sessions.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessGameSessionTicksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GameLoopBackgroundService: Exception occurred during game loop execution.");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessGameSessionTicksAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var activeSessions = await context.GameSessions
            .Where(s => s.State == GameState.Play && s.TargetMonthEndUtc.HasValue)
            .ToListAsync();

        DateTime nowUtc = DateTime.UtcNow;

        foreach (var session in activeSessions)
        {
            if (nowUtc >= session.TargetMonthEndUtc!.Value)
            {
                _logger.LogInformation("GameLoopBackgroundService: Session #{Id} '{Name}' reached target month end. Transitioning to Simulation...", session.Id, session.Name);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var notifier = scope.ServiceProvider.GetRequiredService<IGameSessionNotifierService>();
                    var planNotifier = scope.ServiceProvider.GetRequiredService<IPlanNotifierService>();
                    var orchestrator = scope.ServiceProvider.GetRequiredService<ISimulationOrchestratorService>();
                    var budgetService = scope.ServiceProvider.GetRequiredService<ITeamBudgetService>();

                    // 1. Transition to Simulation State
                    session.State = GameState.Simulation;
                    session.TargetMonthEndUtc = null;
                    session.RemainingSecondsOnPause = null;
                    await context.SaveChangesAsync();
                    await notifier.NotifyGameStateChangedAsync(session);

                    int simMonth = session.CurrentMonth;
                    int nextMonth = simMonth + 1;
                    int simYear = session.StartYear + (simMonth / 12);
                    int monthOfYear = (simMonth % 12) + 1;

                    var costService = scope.ServiceProvider.GetRequiredService<USPSimGame.Services.Costing.ICostCalculationService>();
                    var teamBudgetService = scope.ServiceProvider.GetRequiredService<USPSimGame.Services.Costing.ITeamBudgetService>();
                    var teamNotifier = scope.ServiceProvider.GetRequiredService<USPSimGame.Services.ITeamNotifierService>();

                    // 2. Automate Plan Lifecycle Transitions
                    var sessionPlans = await context.Plans
                        .Include(p => p.Features)
                            .ThenInclude(f => f.GameSessionPlannableLayer)
                                .ThenInclude(l => l!.PlannableLayerDefinition)
                        .Where(p => p.GameSessionId == session.Id)
                        .ToListAsync();

                    bool plansChanged = false;
                    var newlyImplementedPlanIds = new List<int>();
                    foreach (var plan in sessionPlans)
                    {
                        int constructionMonths = await costService.CalculatePlanConstructionTimeMonthsAsync(plan.Id);
                        int constructionStartMonth = plan.StartMonth - constructionMonths;

                        if (nextMonth >= plan.StartMonth)
                        {
                            if (plan.State == PlanState.Approved || plan.State == PlanState.Implementing)
                            {
                                plan.State = PlanState.Implemented;
                                plan.UpdatedAt = DateTime.UtcNow;
                                plansChanged = true;
                                _logger.LogInformation("GameLoopBackgroundService: Plan #{PlanId} '{Name}' transitioned -> Implemented.", plan.Id, plan.Name);

                                // Upfront investment point deductions are deferred until after this
                                // tick's KPI snapshot (see step 3 below), so the balance drop is first
                                // reflected in the snapshot for nextMonth (== plan.StartMonth), matching
                                // the month the plan actually shows as Implemented.
                                newlyImplementedPlanIds.Add(plan.Id);
                            }
                            else if (plan.State == PlanState.Draft || plan.State == PlanState.Consultation || plan.State == PlanState.Requested)
                            {
                                plan.State = PlanState.Archived;
                                plan.UpdatedAt = DateTime.UtcNow;
                                plansChanged = true;
                                _logger.LogInformation("GameLoopBackgroundService: Plan #{PlanId} '{Name}' not approved in time. Transitioned -> Archived.", plan.Id, plan.Name);
                            }
                        }
                        else if (nextMonth >= constructionStartMonth)
                        {
                            if (plan.State == PlanState.Approved)
                            {
                                plan.State = PlanState.Implementing;
                                plan.UpdatedAt = DateTime.UtcNow;
                                plansChanged = true;
                                _logger.LogInformation("GameLoopBackgroundService: Plan #{PlanId} '{Name}' construction started. Transitioned Approved -> Implementing.", plan.Id, plan.Name);
                            }
                            else if (plan.State == PlanState.Draft || plan.State == PlanState.Consultation || plan.State == PlanState.Requested)
                            {
                                plan.State = PlanState.Archived;
                                plan.UpdatedAt = DateTime.UtcNow;
                                plansChanged = true;
                                _logger.LogInformation("GameLoopBackgroundService: Plan #{PlanId} '{Name}' not approved before construction start date. Transitioned -> Archived.", plan.Id, plan.Name);
                            }
                        }
                    }
                    await context.SaveChangesAsync();

                    if (plansChanged)
                    {
                        await planNotifier.NotifyPlansChangedAsync(session.Id);
                    }

                    // 3. Run Simulators & Budget Monthly Tick
                    await orchestrator.RunMonthlySimulationAsync(session.Id, simMonth);
                    await teamBudgetService.ApplyAnnualBudgetRefillAsync(session.Id, monthOfYear);
                    await teamBudgetService.RecordInvestmentPointsSnapshotAsync(session.Id, simMonth);

                    // Execute deferred upfront investment point deductions for plans that just
                    // transitioned to Implemented this tick. Running this after the snapshot above
                    // ensures the deduction is first visible in the NEXT tick's snapshot, which is
                    // labeled nextMonth (== plan.StartMonth) — matching the plan's implementation month.
                    foreach (var planId in newlyImplementedPlanIds)
                    {
                        await teamBudgetService.ExecutePlanImplementationCostsAsync(planId);
                        await teamNotifier.NotifyTeamAreaChangedAsync(session.Id);
                    }

                    await budgetService.ProcessMonthlySimulationTickAsync(session.Id, simYear, monthOfYear);

                    // 4. Advance Month & Return to Play State
                    session.CurrentMonth = simMonth + 1;
                    session.State = GameState.Play;
                    session.TargetMonthEndUtc = DateTime.UtcNow.AddSeconds(session.MonthDurationSeconds <= 0 ? 120 : session.MonthDurationSeconds);
                    session.RemainingSecondsOnPause = null;
                    await context.SaveChangesAsync();

                    _logger.LogInformation("GameLoopBackgroundService: Session #{Id} completed Month #{Month} simulation. Advanced to Month #{NextMonth} in Play state.",
                        session.Id, simMonth, session.CurrentMonth);

                    await notifier.NotifyGameStateChangedAsync(session);
                    await planNotifier.NotifyPlansChangedAsync(session.Id);
                }
            }
        }
    }
}
