using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Game.ControlBarFiles;

public enum StatIconStyle
{
    Dark,
    Accent,
    Plain
}

public partial class Stat : ComponentBase
{
    [Parameter, EditorRequired] public string Icon { get; set; } = string.Empty;
    [Parameter] public StatIconStyle IconStyle { get; set; } = StatIconStyle.Dark;
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;
    [Parameter] public string? Caption { get; set; }
    [Parameter] public string? SmallCaption { get; set; }
    [Parameter, EditorRequired] public string Value { get; set; } = string.Empty;
    [Parameter] public string? Unit { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public bool IsWarning { get; set; }
    [Parameter] public bool IsPreview { get; set; }

    protected string Tooltip
    {
        get
        {
            var title = Title ?? Label;
            return IsPreview ? $"{title} (voorbeeldwaarde)" : title;
        }
    }
}
