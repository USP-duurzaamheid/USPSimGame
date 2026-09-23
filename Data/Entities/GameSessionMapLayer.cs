using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USPSimGame.Data.Entities;

public class GameSessionMapLayer
{
    public int Id { get; set; }

    public int GameSessionId { get; set; }

    [ForeignKey(nameof(GameSessionId))]
    public GameSession GameSession { get; set; } = default!;

    public int MapLayerDefinitionId { get; set; }

    [ForeignKey(nameof(MapLayerDefinitionId))]
    public MapLayerDefinition LayerDefinition { get; set; } = default!;

    public bool IsEnabled { get; set; } = true;

    [Column(TypeName = "text")]
    public string? CachedDataContent { get; set; }

    public string? TranslatorTags { get; set; }

    public string? SimulatorTags { get; set; }

    public DateTime? LastFetchedAt { get; set; }
}
