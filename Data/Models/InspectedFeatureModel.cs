namespace USPSimGame.Data.Models;

public class InspectedFeatureModel
{
    public string FeatureId { get; set; } = string.Empty;
    public string LayerKey { get; set; } = string.Empty;
    public string LayerId { get; set; } = string.Empty;
    public string LayerName { get; set; } = string.Empty;
    public string Category { get; set; } = "Layer";
    public string Color { get; set; } = "#3b82f6";
    public bool IsEditable { get; set; } = false;
    public Dictionary<string, string> Properties { get; set; } = new();
    public List<FeaturePropertyEntry> CustomEntries { get; set; } = new();
}

public class FeaturePropertyEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
