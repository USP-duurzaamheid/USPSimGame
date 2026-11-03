using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;

namespace USPSimGame.Data.Preview;

/// <summary>
/// Example values for parts of the game UI that the backend does not provide yet.
/// Replace a value with real data as soon as the matching backend story is done,
/// and remove it from this class.
/// </summary>
public static class GamePreviewData
{
    /// <summary>Sustainability points earned by completing challenges.</summary>
    public const int SustainabilityPoints = 100;

    /// <summary>Share of renewable electricity production, in percent.</summary>
    public const int RenewableElectricityPercent = 48;

    /// <summary>Number of Campus Credit fines received for missed deadlines.</summary>
    public const int FineCount = 1;

    /// <summary>
    /// Installations causing overlast for the team. Points per unit follow the
    /// "Game Journey" MVP balancing values (solar +1, wind +18, diesel +12).
    /// </summary>
    public static readonly OverlastInfo Overlast = new()
    {
        Limit = 30,
        Sources = new()
        {
            new() { Name = "Zonnepaneel", Icon = "bi-sun", Count = 1, PointsPerUnit = 1 },
            new() { Name = "Dieselgenerator", Icon = "bi-fuel-pump", Count = 1, PointsPerUnit = 12 },
            new() { Name = "Windturbine", Icon = "bi-wind", Count = 0, PointsPerUnit = 18 }
        }
    };

    /// <summary>
    /// Example dilemma that "arrives" shortly after the game screen opens, to show the
    /// new-dilemma pop-up (the first guided challenge from "Game Journey").
    /// </summary>
    public static DilemmaModel GetIncomingDilemma(int currentMonth) => new()
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
    public static List<DilemmaModel> GetDilemmas(int currentMonth) => new()
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
