using USPSimGame.Domain.Entities;

namespace USPSimGame.Application.Services;

public interface IPlayerSessionService
{
    Task<(bool Success, string? ErrorMessage, PlayerSession? PlayerSession, Team? Team, GameSession? GameSession)> ConnectAsync(int teamId, string userName, string password);
}
