namespace USPSimGame.Services.Layers;

public interface IBuildingService
{
    Task<string?> GetBuildingFootprintsGeoJsonAsync(string centerLatLong, double radiusKm = 1.0);
}
