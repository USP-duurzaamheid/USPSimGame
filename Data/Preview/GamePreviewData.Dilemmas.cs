using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;

namespace USPSimGame.Data.Preview;

public static partial class GamePreviewData
{
    /// <summary>
    /// Example dilemma that "arrives" shortly after the game screen opens, to show the
    /// new-dilemma pop-up (the first guided challenge from "Game Journey").
    /// </summary>
    public static DilemmaModel PreviewIncomingDilemma(int currentMonth) => new()
    {
        Code = "I-01",
        Title = "Plaats één zonnepaneel",
        Assignment = "Plaats een zonnepaneel op een toegestane plek in het gebied van je organisatie. Het bouwen duurt 1 maand.",
        Category = DilemmaCategory.Guided,
        DeadlineMonth = currentMonth + 2,
        RewardPoints = 10,
        FineCredits = 5
    };

    /// <summary>
    /// Example dilemmas the team already has, based on the MVP challenge list in "Game Journey".
    /// Deadlines are relative to the current game month so the example stays meaningful
    /// in every session. Rewards and fines are placeholders until the challenge catalogue is final.
    /// </summary>
    public static List<DilemmaModel> PreviewGetCurrentDilemmas(int currentMonth) => new()
    {
        new()
        {
            Code = "I-02",
            Title = "Plaats twee laadpalen",
            Assignment = "Plaats twee laadpalen voor elektrische auto's.",
            Category = DilemmaCategory.Guided,
            DeadlineMonth = currentMonth + 1,
            RewardPoints = 10,
            FineCredits = 5,
            Status = DilemmaStatus.Completed
        },
        new()
        {
            Code = "M-01",
            Title = "Genoeg stroom bij minder zon",
            Assignment = "Zorg dat er genoeg elektriciteit is terwijl zonnepanelen 30% minder opleveren.",
            Category = DilemmaCategory.Medium,
            DeadlineMonth = currentMonth - 1,
            RewardPoints = 25,
            FineCredits = 15,
            Status = DilemmaStatus.Missed
        },
        new()
        {
            Code = "M-03",
            Title = "Samen 50% duurzame stroom",
            Assignment = "Bereik samen met alle organisaties 50% duurzame elektriciteitsproductie op de campus.",
            Category = DilemmaCategory.Medium,
            IsCollaborative = true,
            DeadlineMonth = currentMonth,
            RewardPoints = 30,
            FineCredits = 15
        },
        new()
        {
            Code = "L-01",
            Title = "70% duurzame elektriciteit",
            Assignment = "Bereik 70% duurzame elektriciteitsproductie.",
            Category = DilemmaCategory.LongTerm,
            DeadlineMonth = currentMonth + 36,
            RewardPoints = 60,
            FineCredits = 30
        },
        new()
        {
            Code = "L-02",
            Title = "Opwekcapaciteit van 130%",
            Assignment = "Zorg voor een opwekcapaciteit van 130% van de normale vraag.",
            Category = DilemmaCategory.LongTerm,
            DeadlineMonth = currentMonth + 24,
            RewardPoints = 50,
            FineCredits = 25
        }
    };
}
