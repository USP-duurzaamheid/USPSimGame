using USPSimGame.Data.Models;

namespace USPSimGame.Data.Preview;

public static partial class GamePreviewData
{
    public const string PreviewStoppingFineDilemmaCode = "I-03";

    public static List<FineModel> PreviewGetCurrentFines(int currentMonth) => new()
    {
        new()
        {
            DilemmaCode = PreviewStoppingFineDilemmaCode,
            DilemmaTitle = "Meet a 20% demand increase using a wind turbine",
            Reason = "Deadline missed: the challenge was not completed in time.",
            AmountCredits = 5,
            StartedMonth = currentMonth - 1
        },
        new()
        {
            DilemmaCode = "I-02",
            DilemmaTitle = "Place two charging points",
            Reason = "Deadline missed: the challenge was not completed in time.",
            AmountCredits = 5,
            StartedMonth = currentMonth - 3,
            EndedMonth = currentMonth - 1
        }
    };

    public static FineModel PreviewIncomingFine(int currentMonth) => new()
    {
        DilemmaCode = "M-01",
        DilemmaTitle = "Enough electricity with less sun",
        Reason = "Deadline missed: the challenge was not completed in time.",
        AmountCredits = 15,
        StartedMonth = currentMonth
    };
}
