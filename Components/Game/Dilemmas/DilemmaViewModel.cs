namespace USPSimGame.Components.Game.Dilemmas;

public enum DilemmaCategory
{
    /// <summary>Guided starter challenge (I-01..I-03).</summary>
    Guided,
    /// <summary>Regular medium challenge (M-01..M-04).</summary>
    Medium,
    /// <summary>Long-term goal, tracked in the background (L-01..L-03).</summary>
    LongTerm
}

public enum DilemmaStatus
{
    Active,
    Completed,
    Missed
}

/// <summary>
/// A dilemma (challenge) as shown in the game UI: an assignment from the government
/// with a deadline, a reward in sustainability points and a fine in Campus Credits.
/// </summary>
public record DilemmaViewModel
{
    /// <summary>Short code from the challenge catalogue, e.g. "I-01".</summary>
    public required string Code { get; init; }

    public required string Title { get; init; }

    /// <summary>What the team has to do.</summary>
    public required string Assignment { get; init; }

    public DilemmaCategory Category { get; init; } = DilemmaCategory.Medium;

    /// <summary>True when all organizations work on it together.</summary>
    public bool IsCollaborative { get; init; }

    /// <summary>
    /// Last game month (0 = first month of the session) in which the dilemma can be completed.
    /// A dilemma due in November can still be completed during November.
    /// </summary>
    public int DeadlineMonth { get; init; }

    public int RewardPoints { get; init; }

    public int FineCredits { get; init; }

    public DilemmaStatus Status { get; init; } = DilemmaStatus.Active;

    public int MonthsLeft(int currentMonth) => DeadlineMonth - currentMonth;

    public bool IsMissed(int currentMonth) =>
        Status == DilemmaStatus.Missed || (Status == DilemmaStatus.Active && currentMonth > DeadlineMonth);
}
