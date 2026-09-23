using System.Text.Json.Nodes;
using USPSimGame.Data.Models;

namespace USPSimGame.Services.Layers;

public class PdokSewageWfsService : BaseWfsGeoJsonProvider
{
    public override string ProviderKey => "pdok-gwsw-sewage";
    private const string WfsBaseUrl = "https://service.pdok.nl/rioned/beheer-stedelijk-watersystemen-gwsw/wfs/v1_0";

    public PdokSewageWfsService(HttpClient httpClient, ILogger<PdokSewageWfsService> logger)
        : base(httpClient, logger)
    {
    }

    public override LayerLegendInfo GetLegendInfo()
    {
        return new LayerLegendInfo
        {
            Items = new List<LayerLegendItem>
            {
                new LayerLegendItem { Label = "Sewage & Drainage Mains (BeheerLeiding)", Shape = LegendItemShape.Line, Color = "#059669", LineWidthPx = 3.5 },
                new LayerLegendItem { Label = "Inspection Manholes & Pits (BeheerPut)", Shape = LegendItemShape.Point, Color = "#059669", BorderColor = "#ffffff" },
                new LayerLegendItem { Label = "Pumping Stations & Gemalen (BeheerPomp)", Shape = LegendItemShape.Point, Color = "#ef4444", BorderColor = "#ffffff" }
            }
        };
    }

    public override async Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 1.0)
    {
        var (_, _, rdX, rdY) = ParseCoordinates(centerLatLong);
        string bboxStr = FormatRdBbox(rdX, rdY, Math.Max(radiusKm, 1.5));

        var featureTypes = new[]
        {
            "beheerstedelijkwater:BeheerLeiding",
            "beheerstedelijkwater:BeheerPut",
            "beheerstedelijkwater:BeheerPomp"
        };

        var allFeaturesList = await FetchFeaturesFromTypeNamesAsync(WfsBaseUrl, featureTypes, bboxStr);
        var jsonArray = new JsonArray();
        foreach (var f in allFeaturesList) jsonArray.Add(f);

        var unifiedCollection = new JsonObject
        {
            ["type"] = "FeatureCollection",
            ["name"] = "StedelijkWaterSewageNetwork",
            ["crs"] = new JsonObject
            {
                ["type"] = "name",
                ["properties"] = new JsonObject { ["name"] = "urn:ogc:def:crs:OGC:1.3:CRS84" }
            },
            ["features"] = jsonArray
        };

        return unifiedCollection.ToJsonString();
    }
}
