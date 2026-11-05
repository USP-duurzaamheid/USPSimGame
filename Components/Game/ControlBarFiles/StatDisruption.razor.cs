using System.Globalization;
using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;

namespace USPSimGame.Components.Game.ControlBarFiles;

/// <summary>
/// Noise pollution value in the control bar. Clicking it opens a meter with the total,
/// the limit and which installations cause the noise pollution.
/// </summary>
public partial class StatDisruption : ComponentBase
{
    [Parameter, EditorRequired] public NoisePollutionInfo NoisePollution { get; set; } = default!;

    /// <summary>Marks the values as example data that is not yet provided by the backend.</summary>
    [Parameter] public bool IsPreview { get; set; }

    protected IEnumerable<NoisePollutionSource> OrderedSources =>
        NoisePollution.Sources.OrderByDescending(s => s.SectorNoisePollution).ThenByDescending(s => s.PointsPerUnit);

    protected string LevelClass => NoisePollution.Level.ToString().ToLowerInvariant();

    protected string LevelLabel => NoisePollution.Level switch
    {
        NoisePollutionLevel.High => "Boven de grens",
        NoisePollutionLevel.Medium => "Gemiddeld",
        _ => "Laag"
    };

    /// <summary>The meter shows twice the limit, or more when the total is even higher.</summary>
    protected int MeterMax => Math.Max(Math.Max(NoisePollution.Limit * 2, NoisePollution.OverallNoisePollution), 1);

    protected string FillPercent => ToPercent(NoisePollution.OverallNoisePollution);
    protected string LimitPercent => ToPercent(NoisePollution.Limit);

    private string ToPercent(int value) =>
        (100.0 * value / MeterMax).ToString("0.#", CultureInfo.InvariantCulture) + "%";
}
