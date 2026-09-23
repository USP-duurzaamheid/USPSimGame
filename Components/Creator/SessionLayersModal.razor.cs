using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services.Layers;

namespace USPSimGame.Components.Creator;

public partial class SessionLayersModal : ComponentBase
{
    [Inject]
    public IMapLayerService MapLayerService { get; set; } = default!;

    [Parameter, EditorRequired]
    public GameSession Session { get; set; } = default!;

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback OnUpdated { get; set; }

    protected List<GameSessionMapLayer> SessionMapLayers { get; set; } = new();
    protected List<MapLayerDefinition> AvailableMapDefinitions { get; set; } = new();
    protected int SelectedMapLayerId { get; set; } = 0;

    protected List<GameSessionPlannableLayer> SessionPlannableLayers { get; set; } = new();
    protected List<PlannableLayerDefinition> AvailablePlannableDefinitions { get; set; } = new();
    protected int SelectedPlannableLayerId { get; set; } = 0;

    protected bool IsLoading { get; set; } = true;
    protected bool IsAttachingMapLayer { get; set; } = false;
    protected bool IsAttachingPlannableLayer { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            SessionMapLayers = await MapLayerService.GetSessionLayersAsync(Session.Id);
            AvailableMapDefinitions = await MapLayerService.GetAvailableLayerDefinitionsAsync();

            SessionPlannableLayers = await MapLayerService.GetSessionPlannableLayersAsync(Session.Id);
            AvailablePlannableDefinitions = await MapLayerService.GetAvailablePlannableLayerDefinitionsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionLayersModal] Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task AttachMapLayerAsync()
    {
        if (SelectedMapLayerId > 0)
        {
            IsAttachingMapLayer = true;
            try
            {
                await MapLayerService.AttachLayerToSessionAsync(Session.Id, SelectedMapLayerId);
                SelectedMapLayerId = 0;
                await LoadDataAsync();
                await OnUpdated.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionLayersModal] Error attaching map layer: {ex.Message}");
            }
            finally
            {
                IsAttachingMapLayer = false;
            }
        }
    }

    protected async Task RemoveMapLayerAsync(int sessionLayerId)
    {
        try
        {
            await MapLayerService.RemoveLayerFromSessionAsync(sessionLayerId);
            await LoadDataAsync();
            await OnUpdated.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionLayersModal] Error removing map layer: {ex.Message}");
        }
    }

    protected async Task SaveMapLayerTagsAsync(GameSessionMapLayer layer)
    {
        try
        {
            await MapLayerService.UpdateSessionLayerTagsAsync(layer.Id, layer.TranslatorTags, layer.SimulatorTags);
            await LoadDataAsync();
            await OnUpdated.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionLayersModal] Error saving tags: {ex.Message}");
        }
    }

    protected async Task ResetMapLayerTagsAsync(int sessionLayerId)
    {
        try
        {
            await MapLayerService.ResetSessionLayerTagsToDefaultAsync(sessionLayerId);
            await LoadDataAsync();
            await OnUpdated.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionLayersModal] Error resetting tags: {ex.Message}");
        }
    }

    // --- Plannable Layer Actions ---
    protected async Task AttachPlannableLayerAsync()
    {
        if (SelectedPlannableLayerId > 0)
        {
            IsAttachingPlannableLayer = true;
            try
            {
                await MapLayerService.AttachPlannableLayerToSessionAsync(Session.Id, SelectedPlannableLayerId);
                SelectedPlannableLayerId = 0;
                await LoadDataAsync();
                await OnUpdated.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionLayersModal] Error attaching plannable layer: {ex.Message}");
            }
            finally
            {
                IsAttachingPlannableLayer = false;
            }
        }
    }

    protected async Task RemovePlannableLayerAsync(int sessionPlannableLayerId)
    {
        try
        {
            await MapLayerService.RemovePlannableLayerFromSessionAsync(sessionPlannableLayerId);
            await LoadDataAsync();
            await OnUpdated.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionLayersModal] Error removing plannable layer: {ex.Message}");
        }
    }

    protected async Task CloseAsync()
    {
        await OnClose.InvokeAsync();
    }
}
