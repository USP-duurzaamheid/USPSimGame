using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;

namespace USPSimGame.Data.Preview;

public static partial class GamePreviewData
{
    public static DilemmaModel PreviewIncomingDilemma(int currentMonth) => new()
    {
        Code = "I-01",
        Title = "Place one solar panel",
        Assignment = "Place a solar panel in your organization's installation area. Construction takes 1 month.",
        Category = DilemmaCategory.Guided,
        DeadlineMonth = currentMonth + 2,
        RewardPoints = 10,
        FineCredits = 5
    };

    public static List<DilemmaModel> PreviewGetCurrentDilemmas(int currentMonth) => new()
    {
        new()
        {
            Code = "I-02",
            Title = "Place two charging points",
            Assignment = "Place two charging points for electric cars.",
            Category = DilemmaCategory.Guided,
            DeadlineMonth = currentMonth + 1,
            RewardPoints = 10,
            FineCredits = 5,
            Status = DilemmaStatus.Completed
        },
        new()
        {
            Code = "M-01",
            Title = "Enough electricity with less sun",
            Assignment = "Maintain enough electricity during a 30% solar-output reduction.",
            Category = DilemmaCategory.Medium,
            DeadlineMonth = currentMonth - 1,
            RewardPoints = 25,
            FineCredits = 15,
            Status = DilemmaStatus.Missed
        },
        new()
        {
            Code = "M-03",
            Title = "Together 50% renewable electricity",
            Assignment = "Together reach 50% renewable electricity production across campus.",
            Category = DilemmaCategory.Medium,
            IsCollaborative = true,
            DeadlineMonth = currentMonth,
            RewardPoints = 30,
            FineCredits = 15
        },
        new()
        {
            Code = "L-01",
            Title = "70% renewable electricity",
            Assignment = "Reach 70% renewable electricity production.",
            Category = DilemmaCategory.LongTerm,
            DeadlineMonth = currentMonth + 36,
            RewardPoints = 60,
            FineCredits = 30
        },
        new()
        {
            Code = "L-02",
            Title = "Generation capacity of 130%",
            Assignment = "Reach generation capacity of 130% of normal demand.",
            Category = DilemmaCategory.LongTerm,
            DeadlineMonth = currentMonth + 24,
            RewardPoints = 50,
            FineCredits = 25
        }
    };
}
