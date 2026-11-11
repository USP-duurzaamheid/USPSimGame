using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Models;
using USPSimGame.Services.Costing;

namespace USPSimGame.Components.Game.Plans;

public partial class PlanConstructionProgress : ComponentBase
{
    [Inject] public ICostCalculationService CostCalculationService { get; set; } = default!;

    [Parameter, EditorRequired] public Plan Plan { get; set; } = default!;
    [Parameter] public int CurrentMonth { get; set; }
    [Parameter] public int StartYear { get; set; } = 2026;

    protected ConstructionProgressModel Progress => ConstructionProgressModel.For(
        Plan.StartMonth,
        CurrentMonth,
        CostCalculationService.CalculateDraftPlanConstructionTimeMonths(
            Plan.Features
                .Where(f => f.GameSessionPlannableLayer?.PlannableLayerDefinition != null)
                .Select(f => (f.GameSessionPlannableLayer!.PlannableLayerDefinition, f.GeoJsonGeometry))));

    protected string MonthsLeftLabel => Progress.MonthsLeft switch
    {
        <= 1 => "nog 1 maand",
        var months => $"nog {months} maanden"
    };
}
