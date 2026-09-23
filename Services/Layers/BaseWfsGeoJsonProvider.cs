using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using USPSimGame.Data.Models;
using USPSimGame.Utils;

namespace USPSimGame.Services.Layers;

public abstract class BaseWfsGeoJsonProvider : IMapLayerProvider
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;

    public abstract string ProviderKey { get; }
    public abstract LayerLegendInfo GetLegendInfo();
    public abstract Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 1.0);

    protected BaseWfsGeoJsonProvider(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
    }

    protected (double Lat, double Lon, double RdX, double RdY) ParseCoordinates(string centerLatLong)
    {
        if (!GeoCoordinateConverter.TryParseLatLong(centerLatLong, out double lat, out double lon))
        {
            Logger.LogWarning("[{Provider}] Failed to parse lat/long '{Center}', defaulting to Utrecht Science Park.", ProviderKey, centerLatLong);
            lat = 52.08640;
            lon = 5.17516;
        }

        var (rdX, rdY) = GeoCoordinateConverter.Wgs84ToRd(lat, lon);
        return (lat, lon, rdX, rdY);
    }

    protected string FormatRdBbox(double rdX, double rdY, double radiusKm, bool includeEpsgSuffix = true)
    {
        double radiusMeters = radiusKm * 1000.0;
        double minX = Math.Round(rdX - radiusMeters);
        double maxX = Math.Round(rdX + radiusMeters);
        double minY = Math.Round(rdY - radiusMeters);
        double maxY = Math.Round(rdY + radiusMeters);

        if (includeEpsgSuffix)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{minX:F0},{minY:F0},{maxX:F0},{maxY:F0},EPSG:28992");
        }

        return string.Create(CultureInfo.InvariantCulture, $"{minX:F0},{minY:F0},{maxX:F0},{maxY:F0}");
    }

    protected async Task<string?> HttpGetAsync(string requestUrl, int timeoutSeconds = 15)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            var response = await HttpClient.GetAsync(requestUrl, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                return content;
            }
            Logger.LogWarning("[{Provider}] HTTP request to '{Url}' returned status {Status}", ProviderKey, requestUrl, response.StatusCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[{Provider}] Exception requesting '{Url}'", ProviderKey, requestUrl);
        }

        return null;
    }

    protected async Task<List<JsonNode>> FetchFeaturesFromTypeNamesAsync(string baseUrl, IEnumerable<string> featureTypes, string bboxStr)
    {
        var allFeatures = new List<JsonNode>();
        foreach (var typeName in featureTypes)
        {
            string requestUrl = $"{baseUrl}?service=WFS&version=2.0.0&request=GetFeature&typeName={typeName}&outputFormat=application/json&srsName=EPSG:4326&bbox={bboxStr}";
            var jsonStr = await HttpGetAsync(requestUrl);
            if (!string.IsNullOrWhiteSpace(jsonStr))
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonStr);
                    if (doc.RootElement.TryGetProperty("features", out var featuresElement) && featuresElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var f in featuresElement.EnumerateArray())
                        {
                            var node = JsonNode.Parse(f.GetRawText());
                            if (node != null) allFeatures.Add(node);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[{Provider}] Error parsing GeoJSON for '{TypeName}'", ProviderKey, typeName);
                }
            }
        }
        return allFeatures;
    }
}
