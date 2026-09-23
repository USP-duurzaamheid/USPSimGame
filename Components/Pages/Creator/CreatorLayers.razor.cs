using Microsoft.AspNetCore.Components;
using USPSimGame.Components.Creator;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Pages.Creator;

public partial class CreatorLayers : ComponentBase
{
    protected List<MapLayerDefinition> LayerDefinitions { get; set; } = new();
    protected List<PlannableLayerDefinition> PlannableDefinitions { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string ActiveTab { get; set; } = "baseline";

    // Edit Baseline Tags Modal State
    protected bool ShowEditBaselineModal { get; set; } = false;
    protected MapLayerDefinition? EditingBaselineLayer { get; set; }
    protected string EditTranslatorTags { get; set; } = string.Empty;
    protected string EditSimulatorTags { get; set; } = string.Empty;

    // Create / Edit Plannable Layer Modal State
    protected bool ShowPlannableModal { get; set; } = false;
    protected bool IsEditingPlannable { get; set; } = false;
    protected PlannableLayerDefinition EditingPlannableLayer { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (AuthState.IsAuthenticated)
        {
            await LoadDataAsync();
        }
        else
        {
            IsLoading = false;
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            LayerDefinitions = await MapLayerService.GetAvailableLayerDefinitionsAsync();
            PlannableDefinitions = await MapLayerService.GetAvailablePlannableLayerDefinitionsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreatorLayers] Error loading layer definitions: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void SetTab(string tab)
    {
        ActiveTab = tab;
    }

    // --- Baseline Layer Actions ---
    protected void OpenEditBaselineModal(MapLayerDefinition layer)
    {
        EditingBaselineLayer = layer;
        EditTranslatorTags = layer.TranslatorTags ?? string.Empty;
        EditSimulatorTags = layer.SimulatorTags ?? string.Empty;
        ShowEditBaselineModal = true;
    }

    protected void CloseEditBaselineModal()
    {
        ShowEditBaselineModal = false;
        EditingBaselineLayer = null;
    }

    protected async Task SaveBaselineTagsAsync(EditBaselineTagsModal.TagSavePayload payload)
    {
        if (EditingBaselineLayer != null)
        {
            try
            {
                await MapLayerService.UpdateLayerDefinitionTagsAsync(EditingBaselineLayer.Id, payload.TranslatorTags, payload.SimulatorTags);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreatorLayers] Error saving baseline tags: {ex.Message}");
            }
        }
        CloseEditBaselineModal();
    }

    // --- Plannable Layer Actions ---
    protected void OpenCreatePlannableModal()
    {
        IsEditingPlannable = false;
        EditingPlannableLayer = new PlannableLayerDefinition
        {
            Key = "new-plannable-layer",
            Name = "New Plannable Layer",
            Category = MapLayerCategory.Infrastructure,
            GeometryType = PlannableGeometryType.Polygon,
            Icon = "bi-layers-fill",
            DefaultColor = "#3b82f6",
            DefaultLineWidthPx = 2.5,
            IsEnabledByDefault = false,
            BaseInvestmentPoints = 0,
            InvestmentPointsPerUnit = 30,
            BaseMonthlyExpensePoints = 0,
            MonthlyExpensePointsPerUnit = 1,
            BaseConstructionTimeMonths = 0,
            ConstructionTimeModifierPerUnit = 0
        };
        ShowPlannableModal = true;
    }

    protected void OpenEditPlannableModal(PlannableLayerDefinition def)
    {
        IsEditingPlannable = true;
        EditingPlannableLayer = new PlannableLayerDefinition
        {
            Id = def.Id,
            Key = def.Key,
            Name = def.Name,
            Description = def.Description,
            Category = def.Category,
            GeometryType = def.GeometryType,
            Icon = def.Icon,
            DefaultColor = def.DefaultColor,
            DefaultLineWidthPx = def.DefaultLineWidthPx,
            TranslatorTags = def.TranslatorTags,
            SimulatorTags = def.SimulatorTags,
            IsEnabledByDefault = def.IsEnabledByDefault,
            BaseInvestmentPoints = def.BaseInvestmentPoints,
            InvestmentPointsPerUnit = def.InvestmentPointsPerUnit,
            BaseMonthlyExpensePoints = def.BaseMonthlyExpensePoints,
            MonthlyExpensePointsPerUnit = def.MonthlyExpensePointsPerUnit,
            BaseConstructionTimeMonths = def.BaseConstructionTimeMonths,
            ConstructionTimeModifierPerUnit = def.ConstructionTimeModifierPerUnit
        };
        ShowPlannableModal = true;
    }

    protected void ClosePlannableModal()
    {
        ShowPlannableModal = false;
    }

    protected async Task SavePlannableLayerAsync(PlannableLayerDefinition layer)
    {
        try
        {
            if (IsEditingPlannable)
            {
                await MapLayerService.UpdatePlannableLayerDefinitionAsync(layer);
            }
            else
            {
                await MapLayerService.CreatePlannableLayerDefinitionAsync(layer);
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreatorLayers] Error saving plannable layer: {ex.Message}");
        }
        ClosePlannableModal();
    }

    protected async Task DeletePlannableLayerAsync(int id)
    {
        try
        {
            bool success = await MapLayerService.DeletePlannableLayerDefinitionAsync(id);
            if (success)
            {
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreatorLayers] Error deleting plannable layer: {ex.Message}");
        }
    }
}
