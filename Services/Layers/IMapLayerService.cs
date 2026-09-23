using USPSimGame.Data.Entities;
using USPSimGame.Data.Models;

namespace USPSimGame.Services.Layers;

public interface IMapLayerService
{
    Task<List<MapLayerDefinition>> GetAvailableLayerDefinitionsAsync();
    Task<List<GameSessionMapLayer>> GetSessionLayersAsync(int gameSessionId);
    Task PreFetchAndSaveSessionLayersAsync(int gameSessionId, string centerLatLong, IEnumerable<int>? enabledLayerDefinitionIds = null);
    Task UpdateLayerDefinitionTagsAsync(int definitionId, string? translatorTags, string? simulatorTags);
    Task UpdateSessionLayerTagsAsync(int sessionLayerId, string? translatorTags, string? simulatorTags);
    Task ResetSessionLayerTagsToDefaultAsync(int sessionLayerId);
    Task AttachLayerToSessionAsync(int gameSessionId, int layerDefinitionId);
    Task RemoveLayerFromSessionAsync(int sessionLayerId);
    LayerLegendInfo GetLegendForProvider(string providerKey);

    // Plannable Layer Catalog & Session Management
    Task<List<PlannableLayerDefinition>> GetAvailablePlannableLayerDefinitionsAsync();
    Task CreatePlannableLayerDefinitionAsync(PlannableLayerDefinition def);
    Task UpdatePlannableLayerDefinitionAsync(PlannableLayerDefinition def);
    Task<bool> DeletePlannableLayerDefinitionAsync(int id);
    Task<List<GameSessionPlannableLayer>> GetSessionPlannableLayersAsync(int gameSessionId);
    Task AttachPlannableLayerToSessionAsync(int gameSessionId, int plannableLayerDefinitionId);
    Task RemovePlannableLayerFromSessionAsync(int sessionPlannableLayerId);
    Task UpdateSessionPlannableLayerTagsAsync(int sessionPlannableLayerId, string? translatorTags, string? simulatorTags);
}
