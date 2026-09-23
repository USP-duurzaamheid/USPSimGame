using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Simulation.Modules;

public class SampleEnergySimulatorModule : ISimulatorModule
{
    public string Key => "energy_grid_simulator";
    public string Name => "Energy Grid & Decarbonization Simulator";
    public int ExecutionOrder => 1;
    public List<string> RequiredTags => new() { "pv_generation", "grid_supply" };

    public Task<SimulationOutputResult> ExecuteAsync(SimulationInputContext context)
    {
        var result = new SimulationOutputResult();

        // Use convenience spatial quantity helper methods
        var pvTotals = context.GetAggregateSpatialQuantityBySimulationTag("pv_generation");
        var gridTotals = context.GetAggregateSpatialQuantityBySimulationTag("grid_supply");

        double totalPvAreaM2 = pvTotals.TotalPolygonSquareMeters;
        double gridPointsCount = gridTotals.TotalPointCount;

        // Simulation physics calculations
        double estimatedKwhPerMonth = totalPvAreaM2 * 12.5; // ~12.5 kWh per m² solar panel per month
        double co2SavedTons = Math.Round(estimatedKwhPerMonth * 0.0004, 2); // ~0.4 kg CO2 per kWh
        double peakLoadKw = Math.Max(50.0, 500.0 - (totalPvAreaM2 * 0.08) + (gridPointsCount * 25.0));
        double renewableRatio = Math.Min(100.0, Math.Round((estimatedKwhPerMonth / (estimatedKwhPerMonth + 15000.0)) * 100.0, 1));

        // Global KPIs
        result.Kpis.Add(new SimulationKpiOutput
        {
            GameSessionId = context.GameSessionId,
            SimulatedMonth = context.SimulatedMonth,
            SimulatorKey = Key,
            KpiName = "Renewable Generation",
            Value = Math.Round(estimatedKwhPerMonth, 1),
            Unit = "kWh/mo"
        });

        result.Kpis.Add(new SimulationKpiOutput
        {
            GameSessionId = context.GameSessionId,
            SimulatedMonth = context.SimulatedMonth,
            SimulatorKey = Key,
            KpiName = "Grid Peak Load",
            Value = Math.Round(peakLoadKw, 1),
            Unit = "kW"
        });

        result.Kpis.Add(new SimulationKpiOutput
        {
            GameSessionId = context.GameSessionId,
            SimulatedMonth = context.SimulatedMonth,
            SimulatorKey = Key,
            KpiName = "CO2 Emissions Saved",
            Value = co2SavedTons,
            Unit = "t CO2"
        });

        result.Kpis.Add(new SimulationKpiOutput
        {
            GameSessionId = context.GameSessionId,
            SimulatedMonth = context.SimulatedMonth,
            SimulatorKey = Key,
            KpiName = "Renewable Energy Share",
            Value = renewableRatio,
            Unit = "%"
        });

        // Per-Team KPI breakdown
        foreach (var team in context.SessionTeams)
        {
            var teamPv = context.GetAggregateSpatialQuantityBySimulationTag("pv_generation", team.Id);
            double teamKwh = Math.Round(teamPv.TotalPolygonSquareMeters * 12.5, 1);

            result.Kpis.Add(new SimulationKpiOutput
            {
                GameSessionId = context.GameSessionId,
                SimulatedMonth = context.SimulatedMonth,
                SimulatorKey = Key,
                KpiName = $"Team {team.Name} Solar Generation",
                Value = teamKwh,
                Unit = "kWh/mo",
                TeamId = team.Id
            });
        }

        return Task.FromResult(result);
    }
}
