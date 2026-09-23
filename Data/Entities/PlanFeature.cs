using System.ComponentModel.DataAnnotations.Schema;

namespace USPSimGame.Data.Entities;

public class PlanFeature
{
    public int Id { get; set; }

    public int PlanId { get; set; }

    [ForeignKey(nameof(PlanId))]
    public Plan Plan { get; set; } = default!;

    public int GameSessionPlannableLayerId { get; set; }

    [ForeignKey(nameof(GameSessionPlannableLayerId))]
    public GameSessionPlannableLayer GameSessionPlannableLayer { get; set; } = default!;

    public string? TargetFeatureId { get; set; }

    [Column(TypeName = "text")]
    public string? GeoJsonGeometry { get; set; }

    [Column(TypeName = "text")]
    public string? PropertiesJson { get; set; }

    public bool IsDemolition { get; set; } = false;
}
