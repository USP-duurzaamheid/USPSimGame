namespace USPSimGame.Services.Simulation;

public interface ISimulatorModule
{
    string Key { get; }
    string Name { get; }
    int ExecutionOrder { get; }
    List<string> RequiredTags { get; }

    Task<SimulationOutputResult> ExecuteAsync(SimulationInputContext context);
}
