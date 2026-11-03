using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;

namespace USPSimGame.Components.Game.Hud;

/// <summary>
/// Overlast value in the game top bar. Clicking it opens a meter with the total,
/// the limit and which installations cause the overlast.
/// </summary>
public partial class OverlastIndicator : ComponentBase
{
    [Parameter, EditorRequired] public OverlastInfo Overlast { get; set; } = default!;

    /// <summary>Marks the values as example data that is not yet provided by the backend.</summary>
    [Parameter] public bool IsPreview { get; set; }

    private ElementReference panel;
    private bool focusPanel;

    protected bool IsOpen { get; private set; }

    protected IEnumerable<OverlastSource> OrderedSources =>
        Overlast.Sources.OrderByDescending(s => s.TotalPoints).ThenByDescending(s => s.PointsPerUnit);

    protected string LevelClass => Overlast.Level.ToString().ToLowerInvariant();

    protected string LevelLabel => Overlast.Level switch
    {
        OverlastLevel.High => "Boven de grens",
        OverlastLevel.Medium => "Gemiddeld",
        _ => "Laag"
    };

    /// <summary>The meter shows twice the limit, or more when the total is even higher.</summary>
    protected int MeterMax => Math.Max(Math.Max(Overlast.Limit * 2, Overlast.TotalPoints), 1);

    protected string FillPercent => ToPercent(Overlast.TotalPoints);
    protected string LimitPercent => ToPercent(Overlast.Limit);

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

    private string ToPercent(int value) =>
        (100.0 * value / MeterMax).ToString("0.#", CultureInfo.InvariantCulture) + "%";
}
