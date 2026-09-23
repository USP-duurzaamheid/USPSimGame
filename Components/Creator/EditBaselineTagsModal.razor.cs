using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Creator;

public partial class EditBaselineTagsModal : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public MapLayerDefinition? Layer { get; set; }
    [Parameter] public EventCallback<TagSavePayload> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    protected string TranslatorTags { get; set; } = string.Empty;
    protected string SimulatorTags { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        if (Layer != null)
        {
            TranslatorTags = Layer.TranslatorTags ?? string.Empty;
            SimulatorTags = Layer.SimulatorTags ?? string.Empty;
        }
    }

    protected async Task HandleSaveAsync()
    {
        if (OnSave.HasDelegate)
        {
            await OnSave.InvokeAsync(new TagSavePayload
            {
                TranslatorTags = TranslatorTags,
                SimulatorTags = SimulatorTags
            });
        }
    }

    protected async Task HandleCloseAsync()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }

    public class TagSavePayload
    {
        public string TranslatorTags { get; set; } = string.Empty;
        public string SimulatorTags { get; set; } = string.Empty;
    }
}
