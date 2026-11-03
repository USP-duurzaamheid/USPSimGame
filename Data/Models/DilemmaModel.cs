using USPSimGame.Data.Enums;

namespace USPSimGame.Data.Models;

/// <summary>
/// A dilemma (challenge): an assignment from the government with a deadline,
/// a reward in sustainability points and a fine in Campus Credits.
/// </summary>
public class DilemmaModel
{
    /// <summary>Short code from the challenge catalogue, e.g. "I-01".</summary>
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Assignment { get; set; } = string.Empty;
    public DilemmaCategory Category { get; set; } = DilemmaCategory.Medium;

    /// <summary>True when all organizations work on it together.</summary>
    public bool IsCollaborative { get; set; } = false;

    /// <summary>
    /// Last game month (0 = first month of the session) in which the dilemma can be completed.
    /// A dilemma due in November can still be completed during November.
    /// </summary>
    public int DeadlineMonth { get; set; } = 0;
    public int RewardPoints { get; set; } = 0;
    public int FineCredits { get; set; } = 0;
    public DilemmaStatus Status { get; set; } = DilemmaStatus.Active;

    public int MonthsLeft(int currentMonth) => DeadlineMonth - currentMonth;

    public bool IsMissed(int currentMonth) =>
        Status == DilemmaStatus.Missed || (Status == DilemmaStatus.Active && currentMonth > DeadlineMonth);

    public bool IsDueThisMonth(int currentMonth) =>
        Status == DilemmaStatus.Active && currentMonth == DeadlineMonth;

    public bool NeedsAttention(int currentMonth) => IsMissed(currentMonth) || IsDueThisMonth(currentMonth);
}
