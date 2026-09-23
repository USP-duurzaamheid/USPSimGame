namespace USPSimGame.Services.Simulation;

public class KpiDatasetOption
{
    public string SimulatorKey { get; set; } = string.Empty;
    public string KpiName { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
}

public class KpiDataPoint
{
    public int SimulatedMonth { get; set; }
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}
