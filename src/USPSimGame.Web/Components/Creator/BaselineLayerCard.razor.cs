using Microsoft.AspNetCore.Components;
using USPSimGame.Domain.Entities;

namespace USPSimGame.Web.Components.Creator;

public partial class BaselineLayerCard : ComponentBase
{
    [Parameter] public MapLayerDefinition? Layer { get; set; }
    [Parameter] public EventCallback<MapLayerDefinition> OnEditTags { get; set; }

    protected async Task HandleEditTagsAsync()
    {
        if (Layer != null && OnEditTags.HasDelegate)
        {
            await OnEditTags.InvokeAsync(Layer);
        }
    }
}
