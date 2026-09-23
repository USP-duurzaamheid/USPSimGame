using System.Text.Json;
using System.Text.Json.Nodes;
using USPSimGame.Data.Models;

namespace USPSimGame.Services.Layers;

public class StedinElektraService : IMapLayerProvider
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<StedinElektraService> _logger;

    public string ProviderKey => "stedin-open-data-elektra";

    public StedinElektraService(IWebHostEnvironment env, ILogger<StedinElektraService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public LayerLegendInfo GetLegendInfo()
    {
        return new LayerLegendInfo
        {
            Items = new List<LayerLegendItem>
            {
                new LayerLegendItem { Label = "Low-Voltage Connection Cables (0.4kV)", Shape = LegendItemShape.Line, Color = "#0284c7", LineWidthPx = 3.0 },
                new LayerLegendItem { Label = "Medium-Voltage Distribution Cables (10kV)", Shape = LegendItemShape.Line, Color = "#a855f7", LineWidthPx = 4.0 },
                new LayerLegendItem { Label = "High-Voltage Transmission Network (50kV+)", Shape = LegendItemShape.Line, Color = "#f59e0b", LineWidthPx = 4.5 },
                new LayerLegendItem { Label = "Medium & Low-Voltage Transformer Stations", Shape = LegendItemShape.Point, Color = "#ef4444", BorderColor = "#ffffff" }
            }
        };
    }

    public async Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 1.0)
    {
        _logger.LogInformation("StedinElektraService: Loading and merging all Stedin electricity grid GeoJSON datasets (Cables, Stations, High-Voltage).");

        var fileNames = new[]
        {
            "stedin-laagspanning.json",
            "stedin-middenlaagspanningsstations.json",
            "stedin-hoogspanningsverbindingen.json"
        };

        var allFeatures = new JsonArray();

        foreach (var fileName in fileNames)
        {
            string filePath = Path.Combine(_env.ContentRootPath, "Data", "Layers", fileName);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("StedinElektraService: Dataset file not found at path '{FilePath}'", filePath);
                continue;
            }

            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("features", out var featuresElement) && featuresElement.ValueKind == JsonValueKind.Array)
                {
                    int count = 0;
                    foreach (var f in featuresElement.EnumerateArray())
                    {
                        var node = JsonNode.Parse(f.GetRawText());
                        if (node != null)
                        {
                            allFeatures.Add(node);
                            count++;
                        }
                    }
                    _logger.LogInformation("StedinElektraService: Loaded {Count} features from file '{FileName}'.", count, fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StedinElektraService: Error parsing Stedin GeoJSON dataset file '{FileName}'", fileName);
            }
        }

        var unifiedCollection = new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["name"] = "StedinRegionalElectricityGrid",
            ["crs"] = new JsonObject
            {
                ["type"] = "name",
                ["properties"] = new JsonObject
                {
                    ["name"] = "urn:ogc:def:crs:OGC:1.3:CRS84"
                }
            },
            ["features"] = allFeatures
        };

        string unifiedGeoJson = unifiedCollection.ToJsonString();
        _logger.LogInformation("StedinElektraService: Generated unified Stedin Regional Electricity Grid GeoJSON ({Count} total features, {Length} bytes).", allFeatures.Count, unifiedGeoJson.Length);
        return unifiedGeoJson;
    }
}
