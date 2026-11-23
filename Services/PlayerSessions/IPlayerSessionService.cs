using USPSimGame.Data.Entities;

namespace USPSimGame.Services.PlayerSessions;

public interface IPlayerSessionService
{
    Task<(bool Success, string? ErrorMessage, Team? Team, GameSession? GameSession)> CheckLobby(string joinCode, int teamId, string clientKey);
}
