namespace USPSimGame.Data.Models;

public class FineModel
{
    public string DilemmaCode { get; set; } = string.Empty;
    public string DilemmaTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int AmountCredits { get; set; } = 0;
    public int StartedMonth { get; set; } = 0;
    public int? EndedMonth { get; set; }
    public bool IsActive => EndedMonth == null;
}
