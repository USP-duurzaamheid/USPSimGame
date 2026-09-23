using System.ComponentModel.DataAnnotations;

namespace USPSimGame.Data.Entities;

public class MapLayerDefinition
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public MapLayerType LayerType { get; set; } = MapLayerType.VectorGeoJson;

    public MapLayerCategory Category { get; set; } = MapLayerCategory.Buildings;

    public bool IsEnabledByDefault { get; set; } = true;

    public string? TranslatorTags { get; set; }

    public string? SimulatorTags { get; set; }

    public string? DefaultStyleConfigJson { get; set; }
}
