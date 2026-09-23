namespace USPSimGame.Services.Costing;

public interface ITeamBudgetService
{
    Task ApplyAnnualBudgetRefillAsync(int gameSessionId, int currentMonth);
    Task ProcessMonthlySimulationTickAsync(int gameSessionId, int currentYear, int currentMonth);
    Task ExecutePlanImplementationCostsAsync(int planId);
    Task<double> GetTeamMonthlyExpenseBurdenAsync(int teamId);
    Task RecordInvestmentPointsSnapshotAsync(int gameSessionId, int simulatedMonth);
}
