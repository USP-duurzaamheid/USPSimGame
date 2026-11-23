using System.ComponentModel.DataAnnotations.Schema;

namespace USPSimGame.Data.Entities;

public class PlayerSession
{
    public string? Id { get; set; }
    public int TeamId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastActive { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    public int? GameId { get; set; }

    [ForeignKey(nameof(GameId))]
    public GameSession? GameSession { get; set; }
}


