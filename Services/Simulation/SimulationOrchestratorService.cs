using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Simulation;

public class SimulationOrchestratorService : ISimulationOrchestratorService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IEnumerable<ISimulatorModule> _inProcessModules;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SimulationOrchestratorService> _logger;

    public SimulationOrchestratorService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IEnumerable<ISimulatorModule> inProcessModules,
        IHttpClientFactory httpClientFactory,
        ILogger<SimulationOrchestratorService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _inProcessModules = inProcessModules;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task RunMonthlySimulationAsync(int gameSessionId, int simulatedMonth)
    {
        _logger.LogInformation("SimulationOrchestrator: Starting Month #{Month} simulation run for GameSession #{SessionId}...", simulatedMonth, gameSessionId);

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var session = await context.GameSessions
            .Include(s => s.Teams)
            .FirstOrDefaultAsync(s => s.Id == gameSessionId);

        if (session == null)
        {
            _logger.LogError("SimulationOrchestrator: GameSession #{SessionId} not found.", gameSessionId);
            return;
        }

        // 1. Gather Implemented Plans for this session
        var implementedPlans = await context.Plans
            .Include(p => p.Features)
                .ThenInclude(f => f.GameSessionPlannableLayer)
                    .ThenInclude(g => g.PlannableLayerDefinition)
            .Where(p => p.GameSessionId == gameSessionId && p.State == PlanState.Implemented)
            .ToListAsync();

        // 2. Gather Active Baseline Map Layers
        var activeLayers = await context.GameSessionMapLayers
            .Include(l => l.LayerDefinition)
            .Where(l => l.GameSessionId == gameSessionId && l.IsEnabled)
            .ToListAsync();

        // 3. Gather Prior Simulation KPI and Map Outputs
        var priorKpis = await context.SimulationKpiOutputs
            .Where(k => k.GameSessionId == gameSessionId && k.SimulatedMonth < simulatedMonth)
            .ToListAsync();

        var priorMaps = await context.SimulationMapOutputs
            .Where(m => m.GameSessionId == gameSessionId && m.SimulatedMonth < simulatedMonth)
            .ToListAsync();

        var inputContext = new SimulationInputContext
        {
            GameSessionId = gameSessionId,
            SimulatedMonth = simulatedMonth,
            StartYear = session.StartYear,
            ImplementedPlans = implementedPlans,
            ActiveMapLayers = activeLayers,
            SessionTeams = session.Teams.ToList(),
            PriorKpiOutputs = priorKpis,
            PriorMapOutputs = priorMaps
        };

        // 4. Get Registered Simulator Definitions or Fallback to In-Process Modules
        var moduleDefs = await context.SimulationModuleDefinitions
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.ExecutionOrder)
            .ToListAsync();

        var newKpiOutputs = new List<SimulationKpiOutput>();
        var newMapOutputs = new List<SimulationMapOutput>();

        // Run registered In-Process C# modules first
        foreach (var inProc in _inProcessModules.OrderBy(m => m.ExecutionOrder))
        {
            try
            {
                _logger.LogInformation("SimulationOrchestrator: Running in-process module '{Key}'...", inProc.Key);
                var result = await inProc.ExecuteAsync(inputContext);
                if (result.Kpis != null && result.Kpis.Any()) newKpiOutputs.AddRange(result.Kpis);
                if (result.Maps != null && result.Maps.Any()) newMapOutputs.AddRange(result.Maps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SimulationOrchestrator: Error running in-process module '{Key}'", inProc.Key);
            }
        }

        // Run External REST Simulators if registered in SimulationModuleDefinitions
        foreach (var extDef in moduleDefs.Where(m => m.SimulatorType == SimulatorType.ExternalRest && !string.IsNullOrWhiteSpace(m.EndpointUrlOrPath)))
        {
            try
            {
                _logger.LogInformation("SimulationOrchestrator: Invoking External REST module '{Key}' at '{Url}'...", extDef.Key, extDef.EndpointUrlOrPath);
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.PostAsJsonAsync(extDef.EndpointUrlOrPath, inputContext);
                if (response.IsSuccessStatusCode)
                {
                    var extResult = await response.Content.ReadFromJsonAsync<SimulationOutputResult>();
                    if (extResult?.Kpis != null) newKpiOutputs.AddRange(extResult.Kpis);
                    if (extResult?.Maps != null) newMapOutputs.AddRange(extResult.Maps);
                }
                else
                {
                    _logger.LogWarning("SimulationOrchestrator: External REST module '{Key}' returned status {Code}", extDef.Key, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SimulationOrchestrator: Error invoking external REST module '{Key}'", extDef.Key);
            }
        }

        // 5. Persist Output Results to Database
        if (newKpiOutputs.Any())
        {
            context.SimulationKpiOutputs.AddRange(newKpiOutputs);
        }
        if (newMapOutputs.Any())
        {
            context.SimulationMapOutputs.AddRange(newMapOutputs);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("SimulationOrchestrator: Month #{Month} simulation complete for Session #{SessionId}. Generated {KpiCount} KPIs and {MapCount} Spatial Map outputs.",
            simulatedMonth, gameSessionId, newKpiOutputs.Count, newMapOutputs.Count);
    }
}
