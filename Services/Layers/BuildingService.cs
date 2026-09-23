using System.Globalization;
using USPSimGame.Data.Models;

namespace USPSimGame.Services.Layers;

public class BuildingService : BaseWfsGeoJsonProvider, IBuildingService
{
    public override string ProviderKey => "pdok-3dbag-buildings";

    public BuildingService(HttpClient httpClient, ILogger<BuildingService> logger)
        : base(httpClient, logger)
    {
    }

    public override LayerLegendInfo GetLegendInfo()
    {
        return new LayerLegendInfo
        {
            Items = new List<LayerLegendItem>
            {
                new LayerLegendItem { Label = "Low Kiosks & Sheds (≤ 4m)", Shape = LegendItemShape.PolygonSwatch, Color = "rgba(226, 232, 240, 0.8)", BorderColor = "#cbd5e1" },
                new LayerLegendItem { Label = "Standard Buildings (4m - 34m)", Shape = LegendItemShape.PolygonSwatch, Color = "rgba(238, 242, 252, 0.94)", BorderColor = "#64748b" },
                new LayerLegendItem { Label = "High-Rise Towers (≥ 35m)", Shape = LegendItemShape.PolygonSwatch, Color = "#ffffff", BorderColor = "#334155" }
            }
        };
    }

    public Task<string?> GetBuildingFootprintsGeoJsonAsync(string centerLatLong, double radiusKm = 1.0)
    {
        return FetchLayerDataAsync(centerLatLong, radiusKm);
    }

    public override async Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 1.0)
    {
        var (_, _, rdX, rdY) = ParseCoordinates(centerLatLong);
        string bboxStr = FormatRdBbox(rdX, rdY, radiusKm, includeEpsgSuffix: false);

        string wfsUrl = $"https://data.3dbag.nl/api/BAG3D/wfs?request=GetFeature&service=WFS&version=2.0.0&typename=BAG3D:lod13&outputFormat=json&count=2500&srsName=EPSG:4326&bbox={bboxStr}";
        return await HttpGetAsync(wfsUrl);
    }
}
