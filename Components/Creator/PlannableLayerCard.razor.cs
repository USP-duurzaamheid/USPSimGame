using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Creator;

public partial class PlannableLayerCard : ComponentBase
{
    [Parameter] public PlannableLayerDefinition? Layer { get; set; }
    [Parameter] public EventCallback<PlannableLayerDefinition> OnEdit { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }

    protected async Task HandleEditAsync()
    {
        if (Layer != null && OnEdit.HasDelegate)
        {
            await OnEdit.InvokeAsync(Layer);
        }
    }

    protected async Task HandleDeleteAsync()
    {
        if (Layer != null && OnDelete.HasDelegate)
        {
            await OnDelete.InvokeAsync(Layer.Id);
        }
    }
}
