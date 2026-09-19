using USPSimGame.Application.Models;

namespace USPSimGame.Application.Services.Layers;

public interface IMapLayerProvider
{
    string ProviderKey { get; }
    Task<string?> FetchLayerDataAsync(string centerLatLong, double radiusKm = 1.0);
    LayerLegendInfo GetLegendInfo();
}
