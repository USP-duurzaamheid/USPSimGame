using USPSimGame.Data.Enums;

namespace USPSimGame.Data.Models;

/// <summary>One type of installation that causes overlast (noise).</summary>
public class OverlastSource
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-building";
    public int Count { get; set; } = 0;
    public int PointsPerUnit { get; set; } = 0;

    public int TotalPoints => Count * PointsPerUnit;
}

public class OverlastInfo
{
    public List<OverlastSource> Sources { get; set; } = new();

    /// <summary>Maximum overlast before it counts as too high (challenge M-02 uses 30).</summary>
    public int Limit { get; set; } = 30;

    public int TotalPoints => Sources.Sum(s => s.TotalPoints);

    public OverlastLevel Level =>
        TotalPoints > Limit ? OverlastLevel.High
        : TotalPoints > Limit / 2 ? OverlastLevel.Medium
        : OverlastLevel.Low;
}
