using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Creator;

public partial class PlannableLayerFormModal : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public bool IsEditMode { get; set; }
    [Parameter] public PlannableLayerDefinition? Layer { get; set; }
    [Parameter] public EventCallback<PlannableLayerDefinition> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    protected async Task HandleSaveAsync()
    {
        if (Layer != null && OnSave.HasDelegate)
        {
            await OnSave.InvokeAsync(Layer);
        }
    }

    protected async Task HandleCloseAsync()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }
}
