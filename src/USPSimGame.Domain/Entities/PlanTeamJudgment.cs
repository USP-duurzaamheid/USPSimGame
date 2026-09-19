using System.ComponentModel.DataAnnotations.Schema;
using USPSimGame.Domain.Enums;

namespace USPSimGame.Domain.Entities;

public class PlanTeamJudgment
{
    public int Id { get; set; }

    public int PlanId { get; set; }

    [ForeignKey(nameof(PlanId))]
    public Plan Plan { get; set; } = default!;

    public int TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team Team { get; set; } = default!;

    public PlanJudgmentType Judgment { get; set; } = PlanJudgmentType.Undecided;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
