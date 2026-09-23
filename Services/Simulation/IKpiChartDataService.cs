namespace USPSimGame.Services.Simulation;

public interface IKpiChartDataService
{
    Task<List<KpiDatasetOption>> GetAvailableDatasetsAsync(int gameSessionId, int? scopeToTeamId);
    Task<List<KpiDataPoint>> GetTimeSeriesAsync(int gameSessionId, string simulatorKey, string kpiName, int? teamId, int startYear);
}
