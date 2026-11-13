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
            DilemmaTitle = "Vang 20% extra vraag op met een windturbine",
            Reason = "Deadline gemist: de opdracht was niet op tijd af.",
            AmountCredits = 5,
            StartedMonth = currentMonth - 1
        },
        new()
        {
            DilemmaCode = "I-02",
            DilemmaTitle = "Plaats twee laadpalen",
            Reason = "Deadline gemist: de opdracht was niet op tijd af.",
            AmountCredits = 5,
            StartedMonth = currentMonth - 3,
            EndedMonth = currentMonth - 1
        }
    };

    public static FineModel PreviewIncomingFine(int currentMonth) => new()
    {
        DilemmaCode = "M-01",
        DilemmaTitle = "Genoeg stroom bij minder zon",
        Reason = "Deadline gemist: de opdracht was niet op tijd af.",
        AmountCredits = 15,
        StartedMonth = currentMonth
    };
}
