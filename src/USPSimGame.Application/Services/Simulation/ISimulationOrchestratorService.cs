namespace USPSimGame.Application.Services.Simulation;

public interface ISimulationOrchestratorService
{
    Task RunMonthlySimulationAsync(int gameSessionId, int simulatedMonth);
}
