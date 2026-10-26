using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Game.Hud;

public enum HudIconStyle
{
    /// <summary>Light icon on a dark circle.</summary>
    Dark,
    /// <summary>Dark icon on a yellow circle.</summary>
    Accent,
    /// <summary>Icon without a background circle.</summary>
    Plain
}

/// <summary>
/// One value in the game top bar: an icon followed by a value and optional unit.
/// Renders as a button when <see cref="OnClick"/> is set.
/// </summary>
public partial class HudStat : ComponentBase
{
    [Parameter, EditorRequired] public string Icon { get; set; } = string.Empty;
    [Parameter] public HudIconStyle IconStyle { get; set; } = HudIconStyle.Dark;

    /// <summary>Name of the value, read out by screen readers (e.g. "Campus Credits").</summary>
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;

    /// <summary>Visible text shown before the value (e.g. "Boete").</summary>
    [Parameter] public string? Caption { get; set; }

    [Parameter, EditorRequired] public string Value { get; set; } = string.Empty;
    [Parameter] public string? Unit { get; set; }
    [Parameter] public string? Title { get; set; }

    /// <summary>Highlights the value in red, e.g. for a negative balance.</summary>
    [Parameter] public bool IsWarning { get; set; }

    /// <summary>Marks the value as example data that is not yet provided by the backend.</summary>
    [Parameter] public bool IsPreview { get; set; }

    [Parameter] public EventCallback OnClick { get; set; }

    protected string ToneClass => IsWarning ? "hud-stat--warning" : string.Empty;

    protected string Tooltip
    {
        get
        {
            var title = Title ?? Label;
            return IsPreview ? $"{title} (voorbeeldwaarde)" : title;
        }
    }
}
