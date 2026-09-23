using System.ComponentModel.DataAnnotations.Schema;

namespace USPSimGame.Data.Entities;

public class Team
{
    public int Id { get; set; }
    public int GameSessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;

    public string? AreaDefinition { get; set; }
    public string? LockedBySessionId { get; set; }
    public DateTime? LockedAt { get; set; }

    public double InvestmentPointsBalance { get; set; } = 0;
    public double AnnualBudgetAllowance { get; set; } = 100;

    [NotMapped]
    public string? LockedByUserName { get; set; }
}
