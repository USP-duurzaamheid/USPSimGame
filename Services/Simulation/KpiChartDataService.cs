using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Utils;

namespace USPSimGame.Services.Simulation;

public class KpiChartDataService : IKpiChartDataService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public KpiChartDataService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<KpiDatasetOption>> GetAvailableDatasetsAsync(int gameSessionId, int? scopeToTeamId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var query = context.SimulationKpiOutputs.Where(k => k.GameSessionId == gameSessionId);

        if (scopeToTeamId.HasValue)
        {
            query = query.Where(k => k.TeamId == null || k.TeamId == scopeToTeamId.Value);
        }

        var grouped = await query
            .GroupBy(k => new { k.SimulatorKey, k.KpiName, k.TeamId })
            .Select(g => new
            {
                g.Key.SimulatorKey,
                g.Key.KpiName,
                g.Key.TeamId,
                Unit = g.Max(x => x.Unit)
            })
            .ToListAsync();

        var teamIds = grouped.Where(g => g.TeamId.HasValue).Select(g => g.TeamId!.Value).Distinct().ToList();
        var teamNames = teamIds.Count > 0
            ? await context.Teams.Where(t => teamIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Name)
            : new Dictionary<int, string>();

        var options = grouped
            .Select(g =>
            {
                string? teamName = g.TeamId.HasValue && teamNames.TryGetValue(g.TeamId.Value, out var name) ? name : null;
                return new KpiDatasetOption
                {
                    SimulatorKey = g.SimulatorKey,
                    KpiName = g.KpiName,
                    TeamId = g.TeamId,
                    TeamName = teamName,
                    Unit = g.Unit ?? string.Empty,
                    DisplayLabel = g.TeamId.HasValue ? $"{teamName ?? "Team"}: {g.KpiName}" : g.KpiName
                };
            })
            .OrderBy(o => o.DisplayLabel, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return options;
    }

    public async Task<List<KpiDataPoint>> GetTimeSeriesAsync(int gameSessionId, string simulatorKey, string kpiName, int? teamId, int startYear)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var rows = await context.SimulationKpiOutputs
            .Where(k => k.GameSessionId == gameSessionId
                && k.SimulatorKey == simulatorKey
                && k.KpiName == kpiName
                && k.TeamId == teamId)
            .OrderBy(k => k.SimulatedMonth)
            .ToListAsync();

        return rows.Select(r => new KpiDataPoint
        {
            SimulatedMonth = r.SimulatedMonth,
            Label = CommonGameUtils.FormatMonthYear(r.SimulatedMonth, startYear),
            Value = r.Value,
            Unit = r.Unit
        }).ToList();
    }
}
