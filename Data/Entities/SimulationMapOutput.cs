using System.ComponentModel.DataAnnotations.Schema;

namespace USPSimGame.Data.Entities;

public enum MapOutputDataType
{
    Vector,
    Raster
}

public class SimulationMapOutput
{
    public int Id { get; set; }

    public int GameSessionId { get; set; }

    [ForeignKey(nameof(GameSessionId))]
    public GameSession GameSession { get; set; } = default!;

    public int SimulatedMonth { get; set; }

    public string SimulatorKey { get; set; } = string.Empty;

    public string LayerName { get; set; } = string.Empty;

    public MapOutputDataType DataType { get; set; } = MapOutputDataType.Vector;

    public string GeoJsonOrImageData { get; set; } = string.Empty;

    public string? BoundingBoxJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
