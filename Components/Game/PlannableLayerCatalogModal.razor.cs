using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Game;

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
            PlannableGeometryType.Point => "Point",
            PlannableGeometryType.Line => "Line",
            PlannableGeometryType.Polygon => "Polygon",
            _ => type.ToString()
        };
    }

    protected string GetInvestmentUnitDisplay(PlannableGeometryType type)
    {
        return type switch
        {
            PlannableGeometryType.Point => "pts / point",
            PlannableGeometryType.Line => "pts / 50m",
            PlannableGeometryType.Polygon => "pts / m²",
            _ => "pts / unit"
        };
    }

    protected string GetExpenseUnitDisplay(PlannableGeometryType type)
    {
        return type switch
        {
            PlannableGeometryType.Point => "pts / point / month",
            PlannableGeometryType.Line => "pts / 50m / month",
            PlannableGeometryType.Polygon => "pts / m² / month",
            _ => "pts / unit / month"
        };
    }

}
