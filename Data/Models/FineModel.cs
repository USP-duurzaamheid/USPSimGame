namespace USPSimGame.Data.Models;

/// <summary>A Campus Credit fine for a dilemma whose deadline was missed.</summary>
public class FineModel
{
    public string DilemmaCode { get; set; } = string.Empty;
    public string DilemmaTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int AmountCredits { get; set; } = 0;

    /// <summary>Game month in which the fine started (0 = first month of the session).</summary>
    public int StartedMonth { get; set; } = 0;

    /// <summary>Game month in which the fine stopped; null while it still applies.</summary>
    public int? EndedMonth { get; set; }

    public bool IsActive => EndedMonth == null;
}
