namespace USPSimGame.Data.Entities;

public enum SimulatorType
{
    InProcess,
    LocalExecutable,
    ExternalRest
}

public class SimulationModuleDefinition
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public SimulatorType SimulatorType { get; set; } = SimulatorType.InProcess;

    public int ExecutionOrder { get; set; } = 1;

    public string? EndpointUrlOrPath { get; set; }

    public string RequiredTags { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
}
