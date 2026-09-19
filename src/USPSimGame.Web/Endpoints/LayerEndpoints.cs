using Microsoft.EntityFrameworkCore;
using USPSimGame.Application.Data;
using USPSimGame.Application.Services.Plans;

namespace USPSimGame.Web.Endpoints;

public static class LayerEndpoints
{
    public static IEndpointRouteBuilder MapLayerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/layers/{sessionId:int}/{layerKey}",
            async (int sessionId, string layerKey, AppDbContext db) =>
            {
                var layer = await db.GameSessionMapLayers
                    .Include(item => item.LayerDefinition)
                    .FirstOrDefaultAsync(item =>
                        item.GameSessionId == sessionId &&
                        item.LayerDefinition.Key == layerKey &&
                        item.IsEnabled);

                if (layer == null || string.IsNullOrEmpty(layer.CachedDataContent))
                {
                    return Results.NotFound();
                }

                return Results.Content(layer.CachedDataContent, "application/json");
            });

        endpoints.MapGet(
            "/api/layers/{sessionId:int}/implemented-features",
            async (int sessionId, int? targetMonth, IPlanService planService) =>
            {
                var geoJson = await planService.GetImplementedFeaturesGeoJsonAsync(sessionId, targetMonth);
                return Results.Content(geoJson, "application/json");
            });

        return endpoints;
    }
}
