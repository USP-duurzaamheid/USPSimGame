using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Simulation;

public class SimulationOutputResult
{
    public List<SimulationKpiOutput> Kpis { get; set; } = new();
    public List<SimulationMapOutput> Maps { get; set; } = new();
}
