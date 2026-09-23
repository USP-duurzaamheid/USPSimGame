using System.ComponentModel.DataAnnotations.Schema;

namespace USPSimGame.Data.Entities;

public class SimulationKpiOutput
{
    public int Id { get; set; }

    public int GameSessionId { get; set; }

    [ForeignKey(nameof(GameSessionId))]
    public GameSession GameSession { get; set; } = default!;

    public int SimulatedMonth { get; set; }

    public string SimulatorKey { get; set; } = string.Empty;

    public string KpiName { get; set; } = string.Empty;

    public double Value { get; set; }

    public string Unit { get; set; } = string.Empty;

    public int? TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
