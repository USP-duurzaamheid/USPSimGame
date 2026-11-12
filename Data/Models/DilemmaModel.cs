using USPSimGame.Data.Enums;

namespace USPSimGame.Data.Models;

public class DilemmaModel
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Assignment { get; set; } = string.Empty;
    public DilemmaCategory Category { get; set; } = DilemmaCategory.Medium;
    public bool IsCollaborative { get; set; } = false;

    // Last game month in which the dilemma can still be completed.
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
