using System.Globalization;
using USPSimGame.Data.Models;
using USPSimGame.Utils;

namespace USPSimGame.Services.Layers;

public class PdokKadastraleKaartWfsService : IMapLayerProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PdokKadastraleKaartWfsService> _logger;

    public string ProviderKey => "pdok-brk-kadastralekaart";

    private const string WfsBaseUrl = "https://service.pdok.nl/kadaster/brk-kadastralekaart/wfs/v5_0";

    public PdokKadastraleKaartWfsService(HttpClient httpClient, ILogger<PdokKadastraleKaartWfsService> logger)
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
                new LayerLegendItem { Label = "Cadastral Parcels (Perceelgrenzen)", Shape = LegendItemShape.PolygonSwatch, Color = "rgba(100, 116, 139, 0.12)", BorderColor = "#475569" }
            }
        };
    }

    public async Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 1.0)
    {
        _logger.LogInformation("PdokKadastraleKaartWfsService: Fetching cadastral land parcels via PDOK BRK WFS for center '{Center}'", centerLatLong);

        if (!GeoCoordinateConverter.TryParseLatLong(centerLatLong, out double lat, out double lon))
        {
            _logger.LogWarning("PdokKadastraleKaartWfsService: Invalid center lat/long format '{Center}'. Defaulting to Utrecht Science Park center.", centerLatLong);
            lat = 52.08640;
            lon = 5.17516;
        }

        var (rdX, rdY) = GeoCoordinateConverter.Wgs84ToRd(lat, lon);
        double radiusMeters = Math.Max(radiusKm, 1.0) * 1000;
        double minX = rdX - radiusMeters;
        double maxX = rdX + radiusMeters;
        double minY = rdY - radiusMeters;
        double maxY = rdY + radiusMeters;

        string bboxStr = string.Create(CultureInfo.InvariantCulture, $"{minX:F0},{minY:F0},{maxX:F0},{maxY:F0},EPSG:28992");
        string requestUrl = $"{WfsBaseUrl}?service=WFS&version=2.0.0&request=GetFeature&typeName=kadastralekaart:Perceel&outputFormat=application/json&srsName=EPSG:4326&bbox={bboxStr}";

        try
        {
            _logger.LogInformation("PdokKadastraleKaartWfsService: Requesting BRK WFS URL with BBOX '{Bbox}'...", bboxStr);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var response = await _httpClient.GetAsync(requestUrl, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogInformation("PdokKadastraleKaartWfsService: Successfully loaded BRK Cadastral Parcels GeoJSON ({Length} bytes).", content.Length);
                return content;
            }
            else
            {
                _logger.LogWarning("PdokKadastraleKaartWfsService: WFS query returned status HTTP {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PdokKadastraleKaartWfsService: Exception querying BRK Kadastrale Kaart WFS.");
        }

        return null;
    }
}
