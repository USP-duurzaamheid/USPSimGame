using USPSimGame.Data.Entities;
using USPSimGame.Utils;

namespace USPSimGame.Services.Simulation;

public struct AggregateSpatialQuantity
{
    public double TotalPolygonSquareMeters { get; set; }
    public double TotalLineLengthMeters { get; set; }
    public int TotalPointCount { get; set; }
}

public class SimulationInputContext
{
    public int GameSessionId { get; set; }
    public int SimulatedMonth { get; set; }
    public int StartYear { get; set; }

    public List<Plan> ImplementedPlans { get; set; } = new();
    public List<GameSessionMapLayer> ActiveMapLayers { get; set; } = new();
    public List<Team> SessionTeams { get; set; } = new();

    public List<SimulationKpiOutput> PriorKpiOutputs { get; set; } = new();
    public List<SimulationMapOutput> PriorMapOutputs { get; set; } = new();

    /// <summary>
    /// Gets all GeoJSON geometries from implemented plan features matching the specified simulation tag.
    /// </summary>
    public List<string> GetGeometriesBySimulationTag(string simulationTag, int? teamId = null)
    {
        if (string.IsNullOrWhiteSpace(simulationTag)) return new List<string>();

        var query = ImplementedPlans.AsEnumerable();
        if (teamId.HasValue)
        {
            query = query.Where(p => p.TeamId == teamId.Value);
        }

        var results = new List<string>();
        foreach (var plan in query)
        {
            foreach (var feat in plan.Features)
            {
                var tags = feat.GameSessionPlannableLayer?.PlannableLayerDefinition?.SimulatorTags;
                if (!string.IsNullOrWhiteSpace(tags) &&
                    tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Any(t => t.Equals(simulationTag, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!string.IsNullOrWhiteSpace(feat.GeoJsonGeometry))
                    {
                        results.Add(feat.GeoJsonGeometry);
                    }
                }
            }
        }
        return results;
    }

    /// <summary>
    /// Computes aggregate spatial metrics (m², line meters, point counts) for all implemented plan features matching the specified simulation tag.
    /// </summary>
    public AggregateSpatialQuantity GetAggregateSpatialQuantityBySimulationTag(string simulationTag, int? teamId = null)
    {
        var totals = new AggregateSpatialQuantity();
        if (string.IsNullOrWhiteSpace(simulationTag)) return totals;

        var query = ImplementedPlans.AsEnumerable();
        if (teamId.HasValue)
        {
            query = query.Where(p => p.TeamId == teamId.Value);
        }

        foreach (var plan in query)
        {
            foreach (var feat in plan.Features)
            {
                var def = feat.GameSessionPlannableLayer?.PlannableLayerDefinition;
                if (def == null || string.IsNullOrWhiteSpace(feat.GeoJsonGeometry)) continue;

                var tags = def.SimulatorTags;
                if (!string.IsNullOrWhiteSpace(tags) &&
                    tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Any(t => t.Equals(simulationTag, StringComparison.OrdinalIgnoreCase)))
                {
                    double quantity = GeoJsonSpatialUtils.CalculateFeatureQuantity(feat.GeoJsonGeometry, def.GeometryType);
                    switch (def.GeometryType)
                    {
                        case PlannableGeometryType.Polygon:
                            totals.TotalPolygonSquareMeters += quantity;
                            break;
                        case PlannableGeometryType.Line:
                            totals.TotalLineLengthMeters += (quantity * 50.0); // 1 unit = 50m
                            break;
                        case PlannableGeometryType.Point:
                            totals.TotalPointCount += (int)Math.Round(quantity);
                            break;
                    }
                }
            }
        }

        return totals;
    }
}
