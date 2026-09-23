using System.Globalization;
using USPSimGame.Data.Models;

namespace USPSimGame.Services.Layers;

public class LianderElektraService : IMapLayerProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LianderElektraService> _logger;

    public string ProviderKey => "liander-open-data-elektra";

    public LianderElektraService(HttpClient httpClient, ILogger<LianderElektraService> logger)
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
                new LayerLegendItem { Label = "Low-Voltage Cables (0.4kV)", Shape = LegendItemShape.Line, Color = "#0284c7", LineWidthPx = 3.0 },
                new LayerLegendItem { Label = "Medium-Voltage Cables (10kV)", Shape = LegendItemShape.Line, Color = "#a855f7", LineWidthPx = 4.0 },
                new LayerLegendItem { Label = "High-Voltage Cables (50kV+)", Shape = LegendItemShape.Line, Color = "#f59e0b", LineWidthPx = 5.0 },
                new LayerLegendItem { Label = "Transformer Substation", Shape = LegendItemShape.Point, Color = "#ef4444", BorderColor = "#ffffff" }
            }
        };
    }

    public async Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 1.0)
    {
        _logger.LogInformation("LianderElektraService: Fetching electricity grid data for center '{Center}', radius {Radius}km", centerLatLong, radiusKm);

        if (!USPSimGame.Utils.GeoCoordinateConverter.TryParseLatLong(centerLatLong, out double lat, out double lon))
        {
            _logger.LogWarning("LianderElektraService: Invalid coordinates format '{Center}'", centerLatLong);
            return null;
        }

        // Calculate bounding box in WGS84 degrees for target radius (1.0 km)
        double deltaLat = (radiusKm / 111.0);
        double deltaLon = (radiusKm / (111.0 * Math.Cos(lat * Math.PI / 180.0)));

        double minLat = Math.Round(lat - deltaLat, 6);
        double maxLat = Math.Round(lat + deltaLat, 6);
        double minLon = Math.Round(lon - deltaLon, 6);
        double maxLon = Math.Round(lon + deltaLon, 6);

        // Feature types to query from Liander WFS
        string featureTypes = "Liander_Open_Data_Elektra_WFS:Laagspanningskabel," +
                             "Liander_Open_Data_Elektra_WFS:Middenspanningskabel," +
                             "Liander_Open_Data_Elektra_WFS:Hoogspanningskabel," +
                             "Liander_Open_Data_Elektra_WFS:Middenspanningsstation," +
                             "Liander_Open_Data_Elektra_WFS:Hoogspanningsstation";

        string wfsUrl = $"https://dservices1.arcgis.com/v6W5HAVrpgSg3vts/arcgis/services/Liander_Open_Data_Elektra_WFS/WFSServer" +
                       $"?request=GetFeature&service=WFS&version=2.0.0" +
                       $"&typename={featureTypes}" +
                       $"&outputFormat=GEOJSON&srsName=EPSG:4326" +
                       $"&bbox={minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)},urn:ogc:def:crs:EPSG::4326";

        _logger.LogInformation("LianderElektraService: Querying WFS URL: {Url}", wfsUrl);

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var response = await _httpClient.GetAsync(wfsUrl, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var geoJsonContent = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogInformation("LianderElektraService: Received successful response ({Length} bytes).", geoJsonContent.Length);
                return geoJsonContent;
            }
            else
            {
                _logger.LogWarning("LianderElektraService: WFS request returned HTTP status {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LianderElektraService: Exception during WFS fetch.");
        }

        return null;
    }
}
