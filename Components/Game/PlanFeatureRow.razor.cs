using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Game;

public partial class PlanFeatureRow : ComponentBase
{
    [Parameter] public PlannableLayerDefinition? Definition { get; set; }
    [Parameter] public string? GeoJsonGeometry { get; set; }
    [Parameter] public double? PrecalculatedInvestmentPoints { get; set; }
    [Parameter] public bool IsDemolition { get; set; } = false;
    [Parameter] public bool IsActive { get; set; } = false;
    [Parameter] public bool ShowRemoveButton { get; set; } = false;

    [Parameter] public EventCallback OnSelect { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }

    protected bool HasGeometry => !string.IsNullOrWhiteSpace(GeoJsonGeometry);

    protected double CalculatedInvestmentPoints
    {
        get
        {
            if (PrecalculatedInvestmentPoints.HasValue)
            {
                return PrecalculatedInvestmentPoints.Value;
            }

            if (Definition != null && !string.IsNullOrWhiteSpace(GeoJsonGeometry))
            {
                var est = CostCalculationService.CalculateFeatureCost(Definition, GeoJsonGeometry, IsDemolition);
                return est.TotalInvestmentPoints;
            }

            return 0;
        }
    }

    protected double CalculatedMonthlyExpensePoints
    {
        get
        {
            if (Definition != null && !string.IsNullOrWhiteSpace(GeoJsonGeometry))
            {
                var est = CostCalculationService.CalculateFeatureCost(Definition, GeoJsonGeometry, IsDemolition);
                return est.TotalMonthlyExpensePoints;
            }

            return 0;
        }
    }

    protected async Task HandleSelectAsync()
    {
        if (OnSelect.HasDelegate)
        {
            await OnSelect.InvokeAsync();
        }
    }

    protected async Task HandleRemoveAsync()
    {
        if (OnRemove.HasDelegate)
        {
            await OnRemove.InvokeAsync();
        }
    }
}
