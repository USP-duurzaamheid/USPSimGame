using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace USPSimGame.Components.Game.ControlBarFiles;

public partial class StatPanel : ComponentBase
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Subtitle { get; set; }
    [Parameter, EditorRequired] public RenderFragment StatContent { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private ElementReference panel;
    private bool focusPanel;

    protected bool IsOpen { get; private set; }

    protected void TogglePanel()
    {
        IsOpen = !IsOpen;
        focusPanel = IsOpen;
    }

    protected void ClosePanel() => IsOpen = false;

    protected void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            ClosePanel();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusPanel)
        {
            focusPanel = false;
            await panel.FocusAsync();
        }
    }
}
