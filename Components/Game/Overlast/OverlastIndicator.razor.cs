using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace USPSimGame.Components.Game.Overlast;

public enum OverlastLevel
{
    Low,
    Medium,
    High
}

/// <summary>
/// Overlast value in the game top bar. Clicking it opens a meter with the total,
/// the limit and which installations cause the overlast.
/// </summary>
public partial class OverlastIndicator : ComponentBase
{
    /// <summary>Installations that cause overlast. The total is the sum of all sources.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<OverlastSource> Sources { get; set; } = [];

    /// <summary>Maximum overlast before it counts as too high (e.g. challenge M-02 uses 30).</summary>
    [Parameter] public int Limit { get; set; } = 30;

    /// <summary>Marks the values as example data that is not yet provided by the backend.</summary>
    [Parameter] public bool IsPreview { get; set; }

    private ElementReference panel;
    private bool focusPanel;

    protected bool IsOpen { get; private set; }

    protected int Total => Sources.Sum(s => s.TotalPoints);

    protected IEnumerable<OverlastSource> OrderedSources =>
        Sources.OrderByDescending(s => s.TotalPoints).ThenByDescending(s => s.PointsPerUnit);

    protected OverlastLevel Level =>
        Total > Limit ? OverlastLevel.High
        : Total > Limit / 2 ? OverlastLevel.Medium
        : OverlastLevel.Low;

    protected string LevelClass => Level.ToString().ToLowerInvariant();

    protected string LevelLabel => Level switch
    {
        OverlastLevel.High => "Boven de grens",
        OverlastLevel.Medium => "Gemiddeld",
        _ => "Laag"
    };

    /// <summary>The meter shows twice the limit, or more when the total is even higher.</summary>
    protected int MeterMax => Math.Max(Math.Max(Limit * 2, Total), 1);

    protected string FillPercent => ToPercent(Total);
    protected string LimitPercent => ToPercent(Limit);

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
