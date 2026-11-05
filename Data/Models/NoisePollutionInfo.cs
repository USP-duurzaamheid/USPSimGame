using USPSimGame.Data.Enums;

namespace USPSimGame.Data.Models;

/// <summary>One type of installation that causes noise pollution.</summary>
public class NoisePollutionSource
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-question-diamond-fill";
    public int Count { get; set; } = 0;
    public int PointsPerUnit { get; set; } = 0;

    public int SectorNoisePollution => Count * PointsPerUnit;
}

public class NoisePollutionInfo
{
    public List<NoisePollutionSource> Sources { get; set; } = new();

    /// <summary>Maximum noise pollution before it counts as too high (challenge M-02 uses 30).</summary>
    public int Limit { get; set; } = 30;

    public int OverallNoisePollution => Sources.Sum(s => s.SectorNoisePollution);

    public NoisePollutionLevel Level =>
        OverallNoisePollution > Limit ? NoisePollutionLevel.High
        : OverallNoisePollution > Limit / 2 ? NoisePollutionLevel.Medium
        : NoisePollutionLevel.Low;
}
