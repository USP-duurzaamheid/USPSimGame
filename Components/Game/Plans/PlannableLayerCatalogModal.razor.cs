using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Game.Plans;

public partial class PlannableLayerCatalogModal : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public IEnumerable<GameSessionPlannableLayer>? AvailableLayers { get; set; }
    [Parameter] public HashSet<int> IncludedLayerIds { get; set; } = new();
    [Parameter] public int SelectedLayerId { get; set; }
    [Parameter] public EventCallback<int> OnLayerSelected { get; set; }

    protected async Task Close()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }

    protected async Task SelectLayer(int layerId)
    {
        await Close();
        await OnLayerSelected.InvokeAsync(layerId);
    }

    protected string GetGeometryTypeDisplay(PlannableGeometryType type)
    {
        return type switch
        {
            PlannableGeometryType.Point => "Punt",
            PlannableGeometryType.Line => "Lijn",
            PlannableGeometryType.Polygon => "Vlak",
            _ => type.ToString()
        };
    }

    protected string GetCostDisplay(PlannableLayerDefinition def)
    {
        return GetAmountDisplay(def.GeometryType, def.BaseInvestmentPoints, def.InvestmentPointsPerUnit, string.Empty);
    }

    protected string GetExpenseDisplay(PlannableLayerDefinition def)
    {
        return GetAmountDisplay(def.GeometryType, def.BaseMonthlyExpensePoints, def.MonthlyExpensePointsPerUnit, " per maand");
    }

    protected string GetConstructionTimeDisplay(PlannableLayerDefinition def)
    {
        // A point has one known size; for lines and areas the build time grows with the size.
        if (def.GeometryType == PlannableGeometryType.Point)
        {
            return FormatMonths((int)Math.Ceiling(def.BaseConstructionTimeMonths + def.ConstructionTimeModifierPerUnit));
        }

        return def.ConstructionTimeModifierPerUnit > 0
            ? $"vanaf {FormatMonths(def.BaseConstructionTimeMonths)}"
            : FormatMonths(def.BaseConstructionTimeMonths);
    }

    private static string GetAmountDisplay(PlannableGeometryType type, double baseAmount, double amountPerUnit, string suffix)
    {
        // A point is one unit, so its total is known; for lines and areas it depends on the size.
        return type switch
        {
            PlannableGeometryType.Point => $"{baseAmount + amountPerUnit:0.##} CC{suffix}",
            _ when amountPerUnit == 0 => $"{baseAmount:0.##} CC{suffix}",
            PlannableGeometryType.Line => $"{baseAmount:0.##} CC + {amountPerUnit:0.##} CC / 50m{suffix}",
            _ => $"{baseAmount:0.##} CC + {amountPerUnit:0.##} CC / m²{suffix}"
        };
    }

    private static string FormatMonths(int months) => months == 1 ? "1 maand" : $"{months} maanden";
}
