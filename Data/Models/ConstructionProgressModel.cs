namespace USPSimGame.Data.Models;

/// <summary>How far a plan's construction is, in game months.</summary>
public class ConstructionProgressModel
{
    public int TotalMonths { get; set; } = 0;
    public int MonthsLeft { get; set; } = 0;

    public int PercentComplete => TotalMonths <= 0
        ? 100
        : Math.Clamp(100 * (TotalMonths - MonthsLeft) / TotalMonths, 0, 100);

    /// <param name="completionMonth">Game month in which construction finishes (Plan.StartMonth).</param>
    public static ConstructionProgressModel For(int completionMonth, int currentMonth, int totalMonths) => new()
    {
        TotalMonths = totalMonths,
        MonthsLeft = Math.Max(completionMonth - currentMonth, 0)
    };
}
