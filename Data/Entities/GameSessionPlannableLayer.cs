using System.ComponentModel.DataAnnotations.Schema;

namespace USPSimGame.Data.Entities;

public class GameSessionPlannableLayer
{
    public int Id { get; set; }

    public int GameSessionId { get; set; }

    [ForeignKey(nameof(GameSessionId))]
    public GameSession GameSession { get; set; } = default!;

    public int PlannableLayerDefinitionId { get; set; }

    [ForeignKey(nameof(PlannableLayerDefinitionId))]
    public PlannableLayerDefinition PlannableLayerDefinition { get; set; } = default!;

    public bool IsEnabled { get; set; } = true;
}
