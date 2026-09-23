namespace USPSimGame.Data.Models;

public enum LegendItemShape
{
    Line,
    Point,
    PolygonSwatch
}

public class LayerLegendItem
{
    public string Label { get; set; } = string.Empty;
    public LegendItemShape Shape { get; set; } = LegendItemShape.Line;
    public string Color { get; set; } = "#0284c7";
    public string? BorderColor { get; set; } = "#ffffff";
    public bool IsDashed { get; set; } = false;
    public double LineWidthPx { get; set; } = 3.0;
}

public class LayerLegendInfo
{
    public List<LayerLegendItem> Items { get; set; } = new();
}
