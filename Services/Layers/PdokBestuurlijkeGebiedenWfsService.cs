using System.Globalization;
using USPSimGame.Data.Models;
using USPSimGame.Utils;

namespace USPSimGame.Services.Layers;

public class PdokBestuurlijkeGebiedenWfsService : IMapLayerProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PdokBestuurlijkeGebiedenWfsService> _logger;

    public string ProviderKey => "pdok-brk-bestuurlijkegebieden";

    private const string WfsBaseUrl = "https://service.pdok.nl/kadaster/brk-bestuurlijke-gebieden/wfs/v1_0";

    public PdokBestuurlijkeGebiedenWfsService(HttpClient httpClient, ILogger<PdokBestuurlijkeGebiedenWfsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public LayerLegendInfo GetLegendInfo()
    {
        return new LayerLegendInfo
        {
            Items = new List<LayerLegendItem>
            {
                new LayerLegendItem { Label = "Municipal Jurisdiction Boundaries (Gemeentegrenzen)", Shape = LegendItemShape.Line, Color = "#4f46e5", IsDashed = true, LineWidthPx = 2.5 }
            }
        };
    }

    public async Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 5.0)
    {
        _logger.LogInformation("PdokBestuurlijkeGebiedenWfsService: Fetching municipal administrative boundaries via PDOK WFS for center '{Center}'", centerLatLong);

        if (!GeoCoordinateConverter.TryParseLatLong(centerLatLong, out double lat, out double lon))
        {
            _logger.LogWarning("PdokBestuurlijkeGebiedenWfsService: Invalid center lat/long format '{Center}'. Defaulting to Utrecht Science Park center.", centerLatLong);
            lat = 52.08640;
            lon = 5.17516;
        }

        var (rdX, rdY) = GeoCoordinateConverter.Wgs84ToRd(lat, lon);
        double radiusMeters = Math.Max(radiusKm, 5.0) * 1000;
        double minX = rdX - radiusMeters;
        double maxX = rdX + radiusMeters;
        double minY = rdY - radiusMeters;
        double maxY = rdY + radiusMeters;

        string bboxStr = string.Create(CultureInfo.InvariantCulture, $"{minX:F0},{minY:F0},{maxX:F0},{maxY:F0},EPSG:28992");
        string requestUrl = $"{WfsBaseUrl}?service=WFS&version=2.0.0&request=GetFeature&typeName=bestuurlijkegebieden:Gemeentegebied&outputFormat=application/json&srsName=EPSG:4326&bbox={bboxStr}";

        try
        {
            _logger.LogInformation("PdokBestuurlijkeGebiedenWfsService: Requesting WFS URL with BBOX '{Bbox}'...", bboxStr);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var response = await _httpClient.GetAsync(requestUrl, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogInformation("PdokBestuurlijkeGebiedenWfsService: Successfully loaded BRK Municipal Boundaries GeoJSON ({Length} bytes).", content.Length);
                return content;
            }
            else
            {
                _logger.LogWarning("PdokBestuurlijkeGebiedenWfsService: WFS query returned status HTTP {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PdokBestuurlijkeGebiedenWfsService: Exception querying BRK Bestuurlijke Gebieden WFS.");
        }

        return null;
    }
}
