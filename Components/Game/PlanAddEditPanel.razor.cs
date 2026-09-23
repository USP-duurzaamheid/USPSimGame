using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using USPSimGame.Data.Entities;
using USPSimGame.Services;
using USPSimGame.Services.Layers;
using USPSimGame.Services.Plans;

namespace USPSimGame.Components.Game;

public class DraftFeatureItem
{
    public int GameSessionPlannableLayerId { get; set; }
    public GameSessionPlannableLayer Layer { get; set; } = default!;
    public string? GeoJsonGeometry { get; set; }
    public string? PropertiesJson { get; set; }
    public bool IsDemolition { get; set; } = false;
    public string? TargetFeatureId { get; set; }
}

public partial class PlanAddEditPanel : ComponentBase, IDisposable
{
    [Inject]
    public IPlanService PlanService { get; set; } = default!;

    [Inject]
    public IPlanApprovalEvaluationService EvaluationService { get; set; } = default!;

    [Inject]
    public IMapLayerService MapLayerService { get; set; } = default!;

    [Inject]
    public USPSimGame.Services.Costing.ICostCalculationService CostCalculationService { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    public USPSimGame.Services.PlayerSessionState PlayerSessionState { get; set; } = default!;

    [Parameter, EditorRequired]
    public int GameSessionId { get; set; }

    [Parameter, EditorRequired]
    public int TeamId { get; set; }

    [Parameter]
    public int CurrentPlayerSessionId { get; set; }

    [Parameter]
    public int StartYear { get; set; } = 2026;

    [Parameter]
    public Plan? PlanToEdit { get; set; }

    [Parameter]
    public EventCallback<Plan> OnSaved { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    protected string PlanName { get; set; } = string.Empty;
    protected string Description { get; set; } = string.Empty;

    private int _selectedMonth = 1;
    private int _selectedYear = 2026;

    protected int SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            _selectedMonth = value;
            ValidateSelectedTargetDate();
        }
    }

    protected int SelectedYear
    {
        get => _selectedYear;
        set
        {
            _selectedYear = value;
            ValidateSelectedTargetDate();
        }
    }

    protected int CalculatedConstructionMonths { get; set; } = 0;
    protected int MinimumAllowedCompletionMonth { get; set; } = 0;
    protected string? ConstructionTimeWarningMessage { get; set; }
    protected bool IsConstructionTimeValid { get; set; } = true;

    protected List<GameSessionPlannableLayer> SessionPlannableLayers { get; set; } = new();
    protected int SelectedPlannableLayerId { get; set; } = 0;
    private int _previousSelectedPlannableLayerId = 0;

    protected List<DraftFeatureItem> DraftFeatures { get; set; } = new();

    protected bool IsLoading { get; set; } = true;
    protected bool IsSaving { get; set; } = false;
    protected string? ErrorMessage { get; set; }

    private bool _needsInitialDrawingActivation = false;

    protected bool IsEditMode => PlanToEdit != null;

    protected override async Task OnInitializedAsync()
    {
        if (PlanToEdit != null)
        {
            PlanName = PlanToEdit.Name;
            Description = PlanToEdit.Description ?? string.Empty;

            int totalMonths = (StartYear * 12) + PlanToEdit.StartMonth;
            SelectedYear = totalMonths / 12;
            SelectedMonth = (totalMonths % 12) + 1;
        }
        else
        {
            int currentMonth = PlayerSessionState.CurrentGameSession?.CurrentMonth ?? 0;
            int defaultTargetMonth = currentMonth + 1;

            int totalMonths = (StartYear * 12) + defaultTargetMonth;
            SelectedYear = totalMonths / 12;
            SelectedMonth = (totalMonths % 12) + 1;
        }

        await LoadPlannableLayersAsync();
    }

    protected List<GameSessionPlannableLayer> IncludedLayers { get; set; } = new();
    protected DraftFeatureItem? SelectedDevelopmentFeature { get; set; }

    private async Task LoadPlannableLayersAsync()
    {
        IsLoading = true;
        try
        {
            SessionPlannableLayers = await MapLayerService.GetSessionPlannableLayersAsync(GameSessionId);

            if (PlanToEdit != null && PlanToEdit.Features.Any())
            {
                foreach (var feat in PlanToEdit.Features)
                {
                    var layer = SessionPlannableLayers.FirstOrDefault(l => l.Id == feat.GameSessionPlannableLayerId);
                    if (layer != null)
                    {
                        if (!IncludedLayers.Any(l => l.Id == layer.Id))
                        {
                            IncludedLayers.Add(layer);
                        }

                        if (!string.IsNullOrWhiteSpace(feat.GeoJsonGeometry))
                        {
                            DraftFeatures.Add(new DraftFeatureItem
                            {
                                GameSessionPlannableLayerId = layer.Id,
                                Layer = layer,
                                GeoJsonGeometry = feat.GeoJsonGeometry,
                                PropertiesJson = feat.PropertiesJson,
                                IsDemolition = feat.IsDemolition,
                                TargetFeatureId = feat.TargetFeatureId
                            });
                        }
                    }
                }

                if (IncludedLayers.Any())
                {
                    SelectedPlannableLayerId = IncludedLayers.First().Id;
                }
            }
            else
            {
                SelectedPlannableLayerId = 0;
            }

            _needsInitialDrawingActivation = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error loading plannable layers: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private DotNetObjectReference<PlanAddEditPanel>? _dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            try
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.registerPlanPanelDotNetRef", _dotNetRef);
            }
            catch { }
        }

        if (_needsInitialDrawingActivation)
        {
            _needsInitialDrawingActivation = false;

            if (PlanToEdit != null && DraftFeatures.Any())
            {
                foreach (var item in DraftFeatures)
                {
                    if (item.Layer?.PlannableLayerDefinition != null && !string.IsNullOrWhiteSpace(item.GeoJsonGeometry))
                    {
                        var def = item.Layer.PlannableLayerDefinition;
                        await JSRuntime.InvokeVoidAsync("uspsim2d5.startDrawing", def.GeometryType.ToString(), def.DefaultColor ?? "#3b82f6", "rgba(59, 130, 246, 0.25)", item.GameSessionPlannableLayerId.ToString());
                        await JSRuntime.InvokeVoidAsync("uspsim2d5.loadDraftFeatureGeometry", item.GameSessionPlannableLayerId.ToString(), item.GeoJsonGeometry, item.IsDemolition);
                    }
                }

                if (SelectedPlannableLayerId > 0)
                {
                    await ActivateSelectedLayerDrawingAsync();
                }
            }
        }
    }

    [JSInvokable]
    public async Task OnDraftGeometryUpdated(string layerIdStr, string geoJson)
    {
        if (int.TryParse(layerIdStr, out int layerId))
        {
            var existingAddition = DraftFeatures.FirstOrDefault(f => f.GameSessionPlannableLayerId == layerId && !f.IsDemolition);
            if (existingAddition != null)
            {
                existingAddition.GeoJsonGeometry = string.IsNullOrWhiteSpace(geoJson) ? null : geoJson;
            }
            else if (!string.IsNullOrWhiteSpace(geoJson))
            {
                var layer = SessionPlannableLayers.FirstOrDefault(l => l.Id == layerId);
                if (layer != null)
                {
                    DraftFeatures.Add(new DraftFeatureItem
                    {
                        GameSessionPlannableLayerId = layerId,
                        Layer = layer,
                        GeoJsonGeometry = geoJson,
                        IsDemolition = false
                    });
                }
            }

            await RecalculatePlanCostAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    [JSInvokable]
    public async Task OnDemolitionFeatureAddedFromMap(string layerIdStr, string geoJson, string targetFeatureId)
    {
        if (int.TryParse(layerIdStr, out int layerId))
        {
            var layer = SessionPlannableLayers.FirstOrDefault(l => l.Id == layerId);
            if (layer != null)
            {
                var existingDemo = DraftFeatures.FirstOrDefault(f => f.GameSessionPlannableLayerId == layerId && f.IsDemolition);
                if (existingDemo != null)
                {
                    existingDemo.GeoJsonGeometry = MergeGeoJsonFeatures(existingDemo.GeoJsonGeometry, geoJson);

                    if (!string.IsNullOrEmpty(targetFeatureId))
                    {
                        if (string.IsNullOrEmpty(existingDemo.TargetFeatureId))
                        {
                            existingDemo.TargetFeatureId = targetFeatureId;
                        }
                        else if (!existingDemo.TargetFeatureId.Split(',').Contains(targetFeatureId))
                        {
                            existingDemo.TargetFeatureId += "," + targetFeatureId;
                        }
                    }
                }
                else
                {
                    var demoItem = new DraftFeatureItem
                    {
                        GameSessionPlannableLayerId = layerId,
                        Layer = layer,
                        GeoJsonGeometry = EnsureFeatureCollection(geoJson),
                        IsDemolition = true,
                        TargetFeatureId = targetFeatureId
                    };
                    DraftFeatures.Add(demoItem);
                }

                await RecalculatePlanCostAsync();
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private static string EnsureFeatureCollection(string geoJson)
    {
        if (string.IsNullOrWhiteSpace(geoJson)) return "{\"type\":\"FeatureCollection\",\"features\":[]}";

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(geoJson);
            var root = doc.RootElement;
            if (root.ValueKind == System.Text.Json.JsonValueKind.Object && root.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "FeatureCollection")
            {
                return geoJson;
            }

            var featNode = System.Text.Json.Nodes.JsonNode.Parse(geoJson);
            if (root.ValueKind == System.Text.Json.JsonValueKind.Object && root.TryGetProperty("type", out var tProp) && tProp.GetString() == "Feature")
            {
                var fc = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "FeatureCollection",
                    ["features"] = new System.Text.Json.Nodes.JsonArray { featNode }
                };
                return fc.ToJsonString();
            }

            var feat = new System.Text.Json.Nodes.JsonObject
            {
                ["type"] = "Feature",
                ["geometry"] = featNode,
                ["properties"] = new System.Text.Json.Nodes.JsonObject()
            };
            var featureCollection = new System.Text.Json.Nodes.JsonObject
            {
                ["type"] = "FeatureCollection",
                ["features"] = new System.Text.Json.Nodes.JsonArray { feat }
            };
            return featureCollection.ToJsonString();
        }
        catch
        {
            return geoJson;
        }
    }

    private static string MergeGeoJsonFeatures(string? existingGeoJson, string newGeoJson)
    {
        var fcStr = EnsureFeatureCollection(existingGeoJson ?? "");
        var newFcStr = EnsureFeatureCollection(newGeoJson);

        try
        {
            var existingObj = System.Text.Json.Nodes.JsonNode.Parse(fcStr)?.AsObject();
            var newObj = System.Text.Json.Nodes.JsonNode.Parse(newFcStr)?.AsObject();

            if (existingObj != null && newObj != null)
            {
                var existingFeats = existingObj["features"]?.AsArray() ?? new System.Text.Json.Nodes.JsonArray();
                var newFeats = newObj["features"]?.AsArray() ?? new System.Text.Json.Nodes.JsonArray();

                foreach (var f in newFeats.ToList())
                {
                    if (f != null)
                    {
                        newFeats.Remove(f);
                        existingFeats.Add(f);
                    }
                }
                existingObj["features"] = existingFeats;
                return existingObj.ToJsonString();
            }
        }
        catch { }

        return fcStr;
    }

    public void Dispose()
    {
        try
        {
            JSRuntime.InvokeVoidAsync("uspsim2d5.unregisterPlanPanelDotNetRef");
        }
        catch { }
        _dotNetRef?.Dispose();
    }

    private void EnsureSelectedLayerInDraftFeatures()
    {
        if (SelectedPlannableLayerId > 0)
        {
            var existing = DraftFeatures.FirstOrDefault(f => f.GameSessionPlannableLayerId == SelectedPlannableLayerId);
            if (existing == null)
            {
                var layer = SessionPlannableLayers.FirstOrDefault(l => l.Id == SelectedPlannableLayerId);
                if (layer != null)
                {
                    DraftFeatures.Add(new DraftFeatureItem
                    {
                        GameSessionPlannableLayerId = SelectedPlannableLayerId,
                        Layer = layer,
                        GeoJsonGeometry = null
                    });
                }
            }
        }
    }

    protected bool _isCatalogModalOpen = false;
    protected HashSet<int> _includedLayerIdsSet => IncludedLayers.Select(f => f.Id).ToHashSet();
    protected USPSimGame.Services.Costing.PlanCostEstimate CurrentPlanCost { get; set; }

    protected void OpenCatalogModal()
    {
        _isCatalogModalOpen = true;
    }

    protected async Task RecalculatePlanCostAsync()
    {
        await SyncCurrentLayerGeoJsonAsync();
        var featureTuples = DraftFeatures
            .Where(f => f.Layer?.PlannableLayerDefinition != null && !string.IsNullOrWhiteSpace(f.GeoJsonGeometry))
            .Select(f => (f.Layer.PlannableLayerDefinition, f.GeoJsonGeometry, f.IsDemolition));

        CurrentPlanCost = CostCalculationService.CalculateDraftPlanCost(featureTuples);
    }

    protected async Task OnPlannableLayerChangedAsync()
    {
        await SyncCurrentLayerGeoJsonAsync();
        EnsureSelectedLayerInDraftFeatures();
        await ActivateSelectedLayerDrawingAsync();
        await RecalculatePlanCostAsync();
    }

    protected async Task OnCatalogLayerSelectedAsync(int layerId)
    {
        var layer = SessionPlannableLayers.FirstOrDefault(l => l.Id == layerId);
        if (layer != null)
        {
            if (!IncludedLayers.Any(l => l.Id == layerId))
            {
                IncludedLayers.Add(layer);
            }
            await SelectLayerForDrawingAsync(layerId);
        }
    }

    protected async Task SelectLayerForDrawingAsync(int layerId)
    {
        SelectedDevelopmentFeature = null;
        SelectedPlannableLayerId = layerId;
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.clearPlanHighlight");
        }
        catch { }

        await OnPlannableLayerChangedAsync();
    }

    protected async Task SelectDevelopmentFeatureForHighlightAsync(DraftFeatureItem featureItem)
    {
        SelectedDevelopmentFeature = featureItem;
        SelectedPlannableLayerId = 0;
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.stopDrawing");
            if (featureItem.Layer?.PlannableLayerDefinition != null && !string.IsNullOrWhiteSpace(featureItem.GeoJsonGeometry))
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.renderPlanFeatures", featureItem.GeoJsonGeometry, featureItem.Layer.PlannableLayerDefinition.DefaultColor ?? "#3b82f6");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error highlighting development feature: {ex.Message}");
        }
        StateHasChanged();
    }

    protected async Task RemoveLayerFromPlanAsync(int layerId)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.removeDraftLayer", layerId.ToString());
            await JSRuntime.InvokeVoidAsync("uspsim2d5.clearPlanHighlight");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error removing draft layer in JS: {ex.Message}");
        }

        IncludedLayers.RemoveAll(l => l.Id == layerId);
        DraftFeatures.RemoveAll(f => f.GameSessionPlannableLayerId == layerId);

        if (SelectedPlannableLayerId == layerId)
        {
            var remaining = IncludedLayers.FirstOrDefault();
            if (remaining != null)
            {
                await SelectLayerForDrawingAsync(remaining.Id);
            }
            else
            {
                SelectedPlannableLayerId = 0;
                try
                {
                    await JSRuntime.InvokeVoidAsync("uspsim2d5.stopDrawing");
                }
                catch { }
            }
        }

        await RecalculatePlanCostAsync();
        StateHasChanged();
    }

    protected async Task RemoveDevelopmentFeatureAsync(DraftFeatureItem featureItem)
    {
        if (SelectedDevelopmentFeature == featureItem)
        {
            SelectedDevelopmentFeature = null;
            try
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.clearPlanHighlight");
            }
            catch { }
        }

        DraftFeatures.Remove(featureItem);
        await RecalculatePlanCostAsync();
        StateHasChanged();
    }

    private async Task SyncCurrentLayerGeoJsonAsync()
    {
        try
        {
            foreach (var item in DraftFeatures.Where(f => !f.IsDemolition).ToList())
            {
                string? geoJson = await JSRuntime.InvokeAsync<string?>("uspsim2d5.getDrawnGeoJsonForLayer", item.GameSessionPlannableLayerId.ToString());
                if (!string.IsNullOrWhiteSpace(geoJson))
                {
                    item.GeoJsonGeometry = geoJson;
                }
            }

            if (_previousSelectedPlannableLayerId > 0 && !DraftFeatures.Any(f => f.GameSessionPlannableLayerId == _previousSelectedPlannableLayerId))
            {
                string? prevGeoJson = await JSRuntime.InvokeAsync<string?>("uspsim2d5.getDrawnGeoJsonForLayer", _previousSelectedPlannableLayerId.ToString());
                if (!string.IsNullOrWhiteSpace(prevGeoJson))
                {
                    var layer = SessionPlannableLayers.FirstOrDefault(l => l.Id == _previousSelectedPlannableLayerId);
                    if (layer != null)
                    {
                        DraftFeatures.Add(new DraftFeatureItem
                        {
                            GameSessionPlannableLayerId = _previousSelectedPlannableLayerId,
                            Layer = layer,
                            GeoJsonGeometry = prevGeoJson
                        });
                    }
                }
            }

            await ReevaluateRealtimeSpatialConditionsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error syncing GeoJSON: {ex.Message}");
        }
    }

    protected PlanApprovalEvaluation? RealtimeEvaluation { get; set; }

    protected async Task ReevaluateRealtimeSpatialConditionsAsync()
    {
        try
        {
            var geoms = DraftFeatures
                .Where(f => !string.IsNullOrWhiteSpace(f.GeoJsonGeometry))
                .Select(f => f.GeoJsonGeometry!)
                .ToList();

            RealtimeEvaluation = await EvaluationService.EvaluatePlanGeometryAsync(GameSessionId, TeamId, geoms);

            // Calculate dynamic construction time from draft features
            var featurePairs = DraftFeatures
                .Where(f => f.Layer?.PlannableLayerDefinition != null && !string.IsNullOrWhiteSpace(f.GeoJsonGeometry))
                .Select(f => (f.Layer.PlannableLayerDefinition, f.GeoJsonGeometry));

            CalculatedConstructionMonths = CostCalculationService.CalculateDraftPlanConstructionTimeMonths(featurePairs);

            int currentMonth = PlayerSessionState.CurrentGameSession?.CurrentMonth ?? 0;
            MinimumAllowedCompletionMonth = currentMonth + CalculatedConstructionMonths;

            ValidateSelectedTargetDate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error in spatial evaluation: {ex.Message}");
        }
    }

    protected void ValidateSelectedTargetDate()
    {
        int selectedTargetMonth = ((SelectedYear - StartYear) * 12) + (SelectedMonth - 1);
        if (selectedTargetMonth < MinimumAllowedCompletionMonth)
        {
            IsConstructionTimeValid = false;
            int reqStartMonth = selectedTargetMonth - CalculatedConstructionMonths;
            string reqStartFormatted = USPSimGame.Utils.CommonGameUtils.FormatMonthYear(reqStartMonth, StartYear);
            string minCompFormatted = USPSimGame.Utils.CommonGameUtils.FormatMonthYear(MinimumAllowedCompletionMonth, StartYear);
            string selTargetFormatted = USPSimGame.Utils.CommonGameUtils.FormatMonthYear(selectedTargetMonth, StartYear);

            ConstructionTimeWarningMessage = $"Construction requires {CalculatedConstructionMonths} month(s). Completion by {selTargetFormatted} would require starting in {reqStartFormatted} (in the past). Earliest completion is {minCompFormatted}.";
        }
        else
        {
            IsConstructionTimeValid = true;
            ConstructionTimeWarningMessage = null;
        }
    }

    private async Task ActivateSelectedLayerDrawingAsync()
    {
        var selectedLayer = SessionPlannableLayers.FirstOrDefault(l => l.Id == SelectedPlannableLayerId);
        if (selectedLayer?.PlannableLayerDefinition != null)
        {
            var def = selectedLayer.PlannableLayerDefinition;
            string geomType = def.GeometryType.ToString();
            string color = def.DefaultColor ?? "#3b82f6";
            string layerKey = selectedLayer.Id.ToString();

            try
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.startDrawing", geomType, color, "rgba(59, 130, 246, 0.25)", layerKey);
                _previousSelectedPlannableLayerId = SelectedPlannableLayerId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlanAddEditPanel] Error starting drawing tool for layer {layerKey}: {ex.Message}");
            }
        }
    }

    protected async Task SavePlanAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(PlanName))
        {
            ErrorMessage = "Please enter a name for the plan.";
            return;
        }

        IsSaving = true;
        try
        {
            await SyncCurrentLayerGeoJsonAsync();

            var payloads = DraftFeatures
                .Where(f => !string.IsNullOrWhiteSpace(f.GeoJsonGeometry))
                .Select(f => new PlanFeaturePayload
                {
                    GameSessionPlannableLayerId = f.GameSessionPlannableLayerId,
                    GeoJsonGeometry = f.GeoJsonGeometry,
                    PropertiesJson = f.PropertiesJson,
                    IsDemolition = f.IsDemolition,
                    TargetFeatureId = f.TargetFeatureId
                })
                .ToList();

            if (!payloads.Any())
            {
                ErrorMessage = "Please draw shape geometry for at least one layer before saving.";
                IsSaving = false;
                return;
            }

            ValidateSelectedTargetDate();
            if (!IsConstructionTimeValid)
            {
                ErrorMessage = ConstructionTimeWarningMessage ?? "The selected completion date does not provide enough lead time for construction.";
                IsSaving = false;
                return;
            }

            int calculatedStartMonth = ((SelectedYear - StartYear) * 12) + (SelectedMonth - 1);
            if (calculatedStartMonth < 0) calculatedStartMonth = 0;

            Plan savedPlan;
            if (IsEditMode && PlanToEdit != null)
            {
                savedPlan = await PlanService.UpdatePlanAsync(
                    PlanToEdit.Id,
                    PlanName.Trim(),
                    Description?.Trim(),
                    calculatedStartMonth,
                    payloads
                );
                await PlanService.UnlockPlanAsync(PlanToEdit.Id);
            }
            else
            {
                savedPlan = await PlanService.CreatePlanAsync(
                    GameSessionId,
                    TeamId,
                    PlanName.Trim(),
                    Description?.Trim(),
                    calculatedStartMonth,
                    payloads
                );
            }

            await JSRuntime.InvokeVoidAsync("uspsim2d5.stopDrawing");
            await OnSaved.InvokeAsync(savedPlan);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving plan: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected async Task CancelAsync()
    {
        try
        {
            if (IsEditMode && PlanToEdit != null)
            {
                await PlanService.UnlockPlanAsync(PlanToEdit.Id);
            }
            await JSRuntime.InvokeVoidAsync("uspsim2d5.stopDrawing");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error stopping drawing tool: {ex.Message}");
        }
        await OnCancel.InvokeAsync();
    }

    protected async Task UndoPointAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.undoDrawPoint");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error invoking undoDrawPoint: {ex.Message}");
        }
    }

    protected async Task RedoPointAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.redoDrawPoint");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error invoking redoDrawPoint: {ex.Message}");
        }
    }

    protected async Task DeletePointAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.deleteSelectedVertex");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanAddEditPanel] Error invoking deleteSelectedVertex: {ex.Message}");
        }
    }
}
