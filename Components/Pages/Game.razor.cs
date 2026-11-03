using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenLayers.Blazor;
using USPSimGame.Components.Game;
using USPSimGame.Components.Game.Map;
using USPSimGame.Components.Game.Plans;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;
using USPSimGame.Data.Preview;
using USPSimGame.Services;
using USPSimGame.Services.Auth;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.Teams;
using USPSimGame.Services.Plans;
using USPSimGame.Services.Layers;

namespace USPSimGame.Components.Pages;

public partial class Game : ComponentBase, IDisposable
{
    [Inject]
    public PlayerSessionState PlayerSessionState { get; set; } = default!;

    [Inject]
    public IMapLayerService MapLayerService { get; set; } = default!;

    [Inject]
    public IPlanService PlanService { get; set; } = default!;

    [Inject]
    public ITeamService TeamService { get; set; } = default!;

    [Inject]
    public ITeamNotifierService TeamNotifier { get; set; } = default!;

    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Inject]
    public IGameSessionNotifierService Notifier { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    [Inject]
    public ILogger<Game> Logger { get; set; } = default!;

    private static readonly TimeSpan PreviewDilemmaDelay = TimeSpan.FromSeconds(4);

    protected GameMenu? gameMenu;
    protected PlansControlPanel? plansControlPanel;
    protected MapFeaturePropertiesPanel? propertiesPanel;
    private DotNetObjectReference<Game>? _dotNetRef;
    private readonly CancellationTokenSource _previewDilemmaCts = new();
    private bool _previewDilemmaScheduled;

    protected bool ShowPlanAddEditPanel { get; set; } = false;
    protected Plan? PlanToEdit { get; set; }
    protected Plan? ActivePlan { get; set; }
    protected string? LockErrorMessage { get; set; }
    protected List<Team> SessionTeams { get; set; } = new();

    protected List<DilemmaModel> Dilemmas { get; set; } = new();
    protected DilemmaModel? IncomingDilemma { get; set; }
    protected string? NewDilemmaCode { get; set; }

    protected bool isMapPanelCollapsed { get; set; } = true;

    protected void HandleMapPanelToggle(bool newCollapsed)
    {
        isMapPanelCollapsed = newCollapsed;
    }

    protected void CloseMenuPanelWhenCollapsed(bool isCollapsed)
    {
        if (isCollapsed)
        {
            gameMenu?.CloseSlot();
        }
    }

    /// <summary>
    /// Adds a new dilemma to the team's dilemmas and shows the new-dilemma pop-up.
    /// Call this when the team receives a dilemma during the game.
    /// </summary>
    protected void ReceiveDilemma(DilemmaModel dilemma)
    {
        Dilemmas.RemoveAll(d => d.Code == dilemma.Code);
        Dilemmas.Insert(0, dilemma);
        NewDilemmaCode = dilemma.Code;
        IncomingDilemma = dilemma;
    }

    protected void CloseIncomingDilemma()
    {
        IncomingDilemma = null;
    }

    protected void ViewIncomingDilemma()
    {
        IncomingDilemma = null;
        gameMenu?.OpenDilemmas();
    }

    /// <summary>
    /// Example data: delivers one dilemma shortly after a player opens the game (not during
    /// setup), until the backend sends real dilemmas to <see cref="ReceiveDilemma"/>.
    /// </summary>
    private void SchedulePreviewDilemma()
    {
        var session = PlayerSessionState.CurrentGameSession;
        if (_previewDilemmaScheduled || session == null || PlayerSessionState.IsAdmin || session.State == GameState.Setup)
        {
            return;
        }

        _previewDilemmaScheduled = true;
        _ = DeliverPreviewDilemmaAsync();
    }

    private async Task DeliverPreviewDilemmaAsync()
    {
        try
        {
            await Task.Delay(PreviewDilemmaDelay, _previewDilemmaCts.Token);
            await InvokeAsync(() =>
            {
                ReceiveDilemma(GamePreviewData.GetIncomingDilemma(PlayerSessionState.CurrentGameSession?.CurrentMonth ?? 0));
                StateHasChanged();
            });
        }
        catch (TaskCanceledException)
        {
            // The game screen was closed before the dilemma arrived.
        }
    }

    protected Coordinate InitialCenter { get; private set; } = new Coordinate(5.17516, 52.08640);
    protected double InitialZoom { get; private set; } = 15;

    protected override void OnInitialized()
    {
        var session = PlayerSessionState.CurrentGameSession;
        if (session != null && USPSimGame.Utils.GeoCoordinateConverter.TryParseLatLong(session.CenterLatLong, out double lat, out double lon))
        {
            InitialCenter = new Coordinate(lon, lat);
        }
        else
        {
            Logger.LogWarning("Game.razor: Could not resolve CenterLatLong ('{Center}') for session {SessionId} at OnInitialized. Falling back to Utrecht Science Park default.", session?.CenterLatLong, session?.Id);
        }

        InitialZoom = session?.Zoom ?? 15;
        Dilemmas = GamePreviewData.GetDilemmas(session?.CurrentMonth ?? 0);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogInformation("Game.razor: OnAfterRenderAsync (firstRender=true)");

            if (!PlayerSessionState.IsConnected)
            {
                Logger.LogWarning("Game.razor: PlayerSessionState not connected, navigating to home.");
                Navigation.NavigateTo("/");
                return;
            }

            var session = PlayerSessionState.CurrentGameSession;
            Logger.LogInformation("Game.razor: CurrentGameSession Name='{Name}', CenterLatLong='{Center}'", session?.Name, session?.CenterLatLong);

            if (session != null)
            {
                SchedulePreviewDilemma();

                try
                {
                    Logger.LogInformation("Game.razor: Loading session map layers for Session #{Id}...", session.Id);
                    var layers = await MapLayerService.GetSessionLayersAsync(session.Id);

                    // Fallback: If any active layer has unpopulated cache for session, trigger pre-fetch on demand
                    if ((!layers.Any() || layers.Any(l => l.IsEnabled && string.IsNullOrEmpty(l.CachedDataContent))) && !string.IsNullOrWhiteSpace(session.CenterLatLong))
                    {
                        Logger.LogInformation("Game.razor: Unpopulated layer cache detected for Session {Id}. Triggering pre-fetch on demand...", session.Id);
                        await MapLayerService.PreFetchAndSaveSessionLayersAsync(session.Id, session.CenterLatLong);
                        layers = await MapLayerService.GetSessionLayersAsync(session.Id);
                    }

                    // Trigger browser HTTP stream download for each active session layer
                    foreach (var layer in layers)
                    {
                        if (layer.IsEnabled && !string.IsNullOrEmpty(layer.CachedDataContent))
                        {
                            Logger.LogInformation("Game.razor: Triggering browser HTTP layer stream for '{Key}' in Session #{Id}...", layer.LayerDefinition.Key, session.Id);
                            await JSRuntime.InvokeVoidAsync("uspsim2d5.loadSessionLayer", session.Id, layer.LayerDefinition.Key);
                        }
                    }
                    SessionTeams = await TeamService.GetTeamsByGameSessionAsync(session.Id);
                    TeamNotifier.OnTeamAreaChanged += HandleTeamAreaChangedAsync;
                    Notifier.OnGameSessionStateChanged += HandleGameSessionStateChangedAsync;
                    await JSRuntime.InvokeVoidAsync("uspsim2d5.refreshTeamAreas", session.Id);

                    _dotNetRef = DotNetObjectReference.Create(this);
                    await JSRuntime.InvokeVoidAsync("uspsim2d5.enableMapFeatureInspection", _dotNetRef);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Game.razor: Error triggering session layer HTTP streams.");
                }
            }
        }
    }

    [JSInvokable]
    public void OnMapFeaturesInspected(List<Data.Models.InspectedFeatureModel> clickedFeatures, double x, double y)
    {
        if (clickedFeatures != null && clickedFeatures.Any() && propertiesPanel != null)
        {
            propertiesPanel.Open(clickedFeatures, x, y);
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleTeamAreaChangedAsync(int sessionId)
    {
        if (PlayerSessionState.CurrentGameSession?.Id == sessionId)
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.refreshTeamAreas", sessionId);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleGameSessionStateChangedAsync(GameSession updatedSession)
    {
        if (PlayerSessionState.CurrentGameSession?.Id == updatedSession.Id)
        {
            PlayerSessionState.CurrentGameSession.State = updatedSession.State;
            PlayerSessionState.CurrentGameSession.CurrentMonth = updatedSession.CurrentMonth;
            PlayerSessionState.CurrentGameSession.MonthDurationSeconds = updatedSession.MonthDurationSeconds;
            PlayerSessionState.CurrentGameSession.TargetMonthEndUtc = updatedSession.TargetMonthEndUtc;
            PlayerSessionState.CurrentGameSession.RemainingSecondsOnPause = updatedSession.RemainingSecondsOnPause;
            SchedulePreviewDilemma();
            await InvokeAsync(StateHasChanged);
        }
    }

    protected void OpenNewPlanPanel()
    {
        LockErrorMessage = null;
        ActivePlan = null;
        PlanToEdit = null;
        ShowPlanAddEditPanel = true;
        _ = ClearPlanHighlightAsync();
    }

    protected async Task HandleEditPlanAsync(Plan plan)
    {
        LockErrorMessage = null;
        int currentSessionId = PlayerSessionState.CurrentPlayerSession?.Id ?? 0;

        var (success, errorMsg) = await PlanService.TryLockPlanAsync(plan.Id, currentSessionId);
        if (success)
        {
            ActivePlan = null;
            PlanToEdit = plan;
            ShowPlanAddEditPanel = true;
            await ClearPlanHighlightAsync();
        }
        else
        {
            LockErrorMessage = errorMsg;
        }
    }

    protected async Task ClosePlanAddEditPanelAsync()
    {
        if (PlanToEdit != null)
        {
            await PlanService.UnlockPlanAsync(PlanToEdit.Id);
        }

        ShowPlanAddEditPanel = false;
        PlanToEdit = null;
        await StopDrawingAsync();
    }

    protected async Task HandlePlanSavedAsync(Plan savedPlan)
    {
        ShowPlanAddEditPanel = false;
        PlanToEdit = null;
        await StopDrawingAsync();
        if (plansControlPanel != null)
        {
            await plansControlPanel.RefreshPlansAsync();
        }

        await HandlePlanSelectedAsync(savedPlan);
    }

    protected async Task HandlePlanSelectedAsync(Plan? plan)
    {
        if (ShowPlanAddEditPanel && PlanToEdit != null)
        {
            await PlanService.UnlockPlanAsync(PlanToEdit.Id);
        }

        ShowPlanAddEditPanel = false;
        PlanToEdit = null;
        await StopDrawingAsync();
        ActivePlan = plan;

        if (ActivePlan == null)
        {
            await ClearPlanHighlightAsync();
        }
        else
        {
            await HighlightPlanFeaturesAsync(ActivePlan);
        }
    }

    protected async Task ClosePlanViewAsync()
    {
        ActivePlan = null;
        await ClearPlanHighlightAsync();
    }

    private async Task HighlightPlanFeaturesAsync(Plan plan)
    {
        try
        {
            var featurePayloads = plan.Features
                .Where(f => !string.IsNullOrEmpty(f.GeoJsonGeometry))
                .Select(f => new
                {
                    geoJson = f.GeoJsonGeometry,
                    color = f.GameSessionPlannableLayer?.PlannableLayerDefinition?.DefaultColor ?? "#10b981",
                    layerName = f.GameSessionPlannableLayer?.PlannableLayerDefinition?.Name ?? "Plan Geometry",
                    category = f.GameSessionPlannableLayer?.PlannableLayerDefinition?.Category.ToString() ?? "Spatial Plan",
                    propertiesJson = f.PropertiesJson
                })
                .ToList();

            if (featurePayloads.Any())
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.renderPlanFeatures", featurePayloads, "#10b981");
            }
            else
            {
                await ClearPlanHighlightAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Game.razor: Error rendering plan feature highlight on map.");
        }
    }

    private async Task ClearPlanHighlightAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.clearPlanHighlight");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Game.razor: Error clearing plan highlight.");
        }
    }

    private async Task StopDrawingAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.stopDrawing");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Game.razor: Error stopping drawing mode.");
        }
    }

    public void Dispose()
    {
        TeamNotifier.OnTeamAreaChanged -= HandleTeamAreaChangedAsync;
        Notifier.OnGameSessionStateChanged -= HandleGameSessionStateChangedAsync;
        _dotNetRef?.Dispose();
        _previewDilemmaCts.Cancel();
        _previewDilemmaCts.Dispose();
    }
}
