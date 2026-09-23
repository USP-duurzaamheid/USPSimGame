using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using USPSimGame.Data.Entities;
using USPSimGame.Services;
using USPSimGame.Services.Layers;

namespace USPSimGame.Components.Game;

public class MapLayerOrderItem
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BadgeClass { get; set; } = "bg-secondary-subtle text-secondary";
    public bool IsSpecial { get; set; } = false;
    public GameSessionMapLayer? SessionMapLayer { get; set; }
}

public partial class MapControlPanel : ComponentBase
{
    [Inject]
    public IMapLayerService MapLayerService { get; set; } = default!;

    [Inject]
    public ITeamService TeamService { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    public IPlanNotifierService PlanNotifier { get; set; } = default!;

    [Parameter, EditorRequired]
    public int GameSessionId { get; set; }

    protected List<GameSessionMapLayer> SessionLayers { get; set; } = new();
    protected List<GameSessionPlannableLayer> SessionPlannableLayers { get; set; } = new();
    protected List<Team> SessionTeams { get; set; } = new();
    protected Dictionary<string, bool> LayerVisibilities { get; set; } = new();
    protected List<MapLayerOrderItem> OrderedVisibleLayers { get; set; } = new();

    [Parameter]
    public bool IsCollapsed { get; set; } = true;

    [Parameter]
    public EventCallback<bool> OnToggleCollapse { get; set; }

    protected bool ShowTeamAreas { get; set; } = true;
    protected bool ShowImplementedFeatures { get; set; } = true;
    protected string ActiveTab { get; set; } = "layers";
    protected int? DraggedIndex { get; set; }

    protected int TotalLayersCount => SessionLayers.Count + 2;

    protected int VisibleCount => LayerVisibilities.Values.Count(v => v) + (ShowTeamAreas ? 1 : 0) + (ShowImplementedFeatures ? 1 : 0);

    protected override void OnInitialized()
    {
        PlanNotifier.OnPlansChanged += HandlePlansChangedAsync;
    }

    private async Task HandlePlansChangedAsync(int gameSessionId)
    {
        if (GameSessionId == gameSessionId)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.loadSessionImplementedFeatures", GameSessionId);
            }
            catch { }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.loadSessionImplementedFeatures", GameSessionId);
            }
            catch { }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadSessionLayersAsync();
        await LoadSessionTeamsAsync();
        await LoadSessionPlannableLayersAsync();
    }

    private async Task LoadSessionTeamsAsync()
    {
        try
        {
            SessionTeams = await TeamService.GetTeamsByGameSessionAsync(GameSessionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapControlPanel] Error loading session teams: {ex.Message}");
        }
    }

    private async Task LoadSessionPlannableLayersAsync()
    {
        try
        {
            SessionPlannableLayers = await MapLayerService.GetSessionPlannableLayersAsync(GameSessionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapControlPanel] Error loading session plannable layers: {ex.Message}");
        }
    }

    private async Task LoadSessionLayersAsync()
    {
        try
        {
            SessionLayers = await MapLayerService.GetSessionLayersAsync(GameSessionId);

            foreach (var layer in SessionLayers)
            {
                if (!LayerVisibilities.ContainsKey(layer.LayerDefinition.Key))
                {
                    bool isDefaultOn = layer.LayerDefinition.Key == "pdok-3dbag-buildings" || layer.LayerDefinition.IsEnabledByDefault;
                    LayerVisibilities[layer.LayerDefinition.Key] = isDefaultOn;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapControlPanel] Error loading session layers: {ex.Message}");
        }
        await UpdateOrderedVisibleLayersAsync();
    }

    protected async Task ToggleCollapse()
    {
        IsCollapsed = !IsCollapsed;
        await OnToggleCollapse.InvokeAsync(IsCollapsed);
    }

    protected async Task ToggleTeamAreasVisibilityAsync(bool isVisible)
    {
        ShowTeamAreas = isVisible;
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.toggleTeamAreasVisibility", isVisible);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapControlPanel] JS Error toggling team areas visibility: {ex.Message}");
        }
        await UpdateOrderedVisibleLayersAsync();
    }

    protected async Task ToggleImplementedFeaturesVisibilityAsync(bool isVisible)
    {
        ShowImplementedFeatures = isVisible;
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.toggleImplementedFeaturesVisibility", isVisible);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapControlPanel] JS Error toggling implemented features visibility: {ex.Message}");
        }
        await UpdateOrderedVisibleLayersAsync();
    }

    protected async Task ToggleLayerVisibilityAsync(string layerKey, bool isVisible)
    {
        LayerVisibilities[layerKey] = isVisible;
        await UpdateOrderedVisibleLayersAsync();

        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.setLayerVisibility", layerKey, isVisible);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapControlPanel] JS Error setting visibility for '{layerKey}': {ex.Message}");
        }
    }

    private async Task UpdateOrderedVisibleLayersAsync()
    {
        var visibleKeys = new HashSet<string>();
        if (ShowTeamAreas) visibleKeys.Add("team-areas-layer");
        if (ShowImplementedFeatures) visibleKeys.Add("implemented-features-layer");
        foreach (var kvp in LayerVisibilities.Where(k => k.Value))
        {
            visibleKeys.Add(kvp.Key);
        }

        // Remove layers no longer visible
        OrderedVisibleLayers.RemoveAll(item => !visibleKeys.Contains(item.Key));

        // Add newly visible session layers to the END of the list (below special layers)
        foreach (var layer in SessionLayers)
        {
            if (LayerVisibilities.TryGetValue(layer.LayerDefinition.Key, out bool isVis) && isVis)
            {
                if (!OrderedVisibleLayers.Any(i => i.Key == layer.LayerDefinition.Key))
                {
                    OrderedVisibleLayers.Add(new MapLayerOrderItem
                    {
                        Key = layer.LayerDefinition.Key,
                        Name = layer.LayerDefinition.Name,
                        Category = layer.LayerDefinition.Category.ToString(),
                        BadgeClass = "bg-secondary-subtle text-secondary",
                        SessionMapLayer = layer
                    });
                }
            }
        }

        // Ensure Existing Team Areas is inserted as second top layer (or top if Implemented Developments is hidden)
        if (ShowTeamAreas && !OrderedVisibleLayers.Any(i => i.Key == "team-areas-layer"))
        {
            int insertIndex = OrderedVisibleLayers.Any(i => i.Key == "implemented-features-layer") ? 1 : 0;
            OrderedVisibleLayers.Insert(insertIndex, new MapLayerOrderItem
            {
                Key = "team-areas-layer",
                Name = "Existing Team Areas",
                Category = "Territories",
                BadgeClass = "bg-primary-subtle text-primary",
                IsSpecial = true
            });
        }

        // Ensure Implemented Developments is inserted at index 0 (very TOP layer)
        if (ShowImplementedFeatures && !OrderedVisibleLayers.Any(i => i.Key == "implemented-features-layer"))
        {
            OrderedVisibleLayers.Insert(0, new MapLayerOrderItem
            {
                Key = "implemented-features-layer",
                Name = "Implemented Developments",
                Category = "Active Developments",
                BadgeClass = "bg-success-subtle text-success",
                IsSpecial = true
            });
        }

        await ReassignZIndicesAsync();
    }

    protected void HandleDragStart(int index)
    {
        DraggedIndex = index;
    }

    protected async Task HandleDropAsync(int targetIndex)
    {
        if (DraggedIndex.HasValue && DraggedIndex.Value != targetIndex)
        {
            var item = OrderedVisibleLayers[DraggedIndex.Value];
            OrderedVisibleLayers.RemoveAt(DraggedIndex.Value);
            OrderedVisibleLayers.Insert(targetIndex, item);
            DraggedIndex = null;

            await ReassignZIndicesAsync();
        }
    }

    protected async Task MoveLayerUpAsync(int index)
    {
        if (index > 0)
        {
            var item = OrderedVisibleLayers[index];
            OrderedVisibleLayers.RemoveAt(index);
            OrderedVisibleLayers.Insert(index - 1, item);

            await ReassignZIndicesAsync();
        }
    }

    protected async Task MoveLayerDownAsync(int index)
    {
        if (index < OrderedVisibleLayers.Count - 1)
        {
            var item = OrderedVisibleLayers[index];
            OrderedVisibleLayers.RemoveAt(index);
            OrderedVisibleLayers.Insert(index + 1, item);

            await ReassignZIndicesAsync();
        }
    }

    private async Task ReassignZIndicesAsync()
    {
        int baseZIndex = 200;
        for (int i = 0; i < OrderedVisibleLayers.Count; i++)
        {
            int assignedZIndex = baseZIndex - (i * 10);
            var key = OrderedVisibleLayers[i].Key;

            try
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.setLayerZIndex", key, assignedZIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapControlPanel] JS Error setting zIndex for '{key}': {ex.Message}");
            }
        }
    }
}
