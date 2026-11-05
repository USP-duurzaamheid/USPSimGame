using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace USPSimGame.Components.Game.ControlBarFiles;

/// <summary>
/// A control bar value that opens a details panel when clicked. The panel closes with
/// Escape, its close button or a click outside it.
/// </summary>
public partial class StatPanel : ComponentBase
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Subtitle { get; set; }

    /// <summary>The value shown in the control bar.</summary>
    [Parameter, EditorRequired] public RenderFragment StatContent { get; set; } = default!;

    /// <summary>The content of the details panel.</summary>
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
