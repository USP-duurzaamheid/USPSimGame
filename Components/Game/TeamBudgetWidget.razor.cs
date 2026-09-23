using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;
using USPSimGame.Services.Costing;

namespace USPSimGame.Components.Game;

public partial class TeamBudgetWidget : ComponentBase, IDisposable
{
    [Inject] public ITeamService TeamService { get; set; } = default!;
    [Inject] public ITeamBudgetService TeamBudgetService { get; set; } = default!;
    [Inject] public IGameSessionNotifierService SessionNotifier { get; set; } = default!;
    [Inject] public IPlanNotifierService PlanNotifier { get; set; } = default!;

    [Parameter, EditorRequired] public int TeamId { get; set; }

    protected double CurrentBalance { get; set; } = 100;
    protected double MonthlyExpenseBurden { get; set; } = 0;

    private int _loadedTeamId = -1;

    protected override void OnInitialized()
    {
        SessionNotifier.OnGameSessionStateChanged += HandleSessionStateChangedAsync;
        PlanNotifier.OnPlansChanged += HandlePlansChangedAsync;
    }

    protected override async Task OnParametersSetAsync()
    {
        // Note: this component's parent (GameOverlay) re-renders every second to drive a
        // countdown timer, which would otherwise re-trigger a DB reload here every second too.
        // Only reload when the TeamId parameter actually changes; ongoing balance updates are
        // driven exclusively by the notifier events below, which fire once per real change
        // after all DB writes for that tick are committed. Reloading on every render created a
        // race where an old, slow-to-complete per-second read could overwrite a newer, correct
        // notifier-triggered read (e.g. right after the annual budget refill).
        if (TeamId != _loadedTeamId)
        {
            _loadedTeamId = TeamId;
            await LoadBudgetStatusAsync();
        }
    }

    private async Task HandleSessionStateChangedAsync(GameSession session)
    {
        await InvokeAsync(RefreshAsync);
    }

    private async Task HandlePlansChangedAsync(int gameSessionId)
    {
        await InvokeAsync(RefreshAsync);
    }

    public async Task RefreshAsync()
    {
        await LoadBudgetStatusAsync();
        StateHasChanged();
    }

    private async Task LoadBudgetStatusAsync()
    {
        if (TeamId <= 0) return;
        try
        {
            var team = await TeamService.GetTeamByIdAsync(TeamId);
            if (team != null)
            {
                CurrentBalance = team.InvestmentPointsBalance;
            }
            MonthlyExpenseBurden = await TeamBudgetService.GetTeamMonthlyExpenseBurdenAsync(TeamId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeamBudgetWidget] Error loading budget status: {ex.Message}");
        }
    }

    public void Dispose()
    {
        SessionNotifier.OnGameSessionStateChanged -= HandleSessionStateChangedAsync;
        PlanNotifier.OnPlansChanged -= HandlePlansChangedAsync;
    }
}
