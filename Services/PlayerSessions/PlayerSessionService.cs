using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;
using USPSimGame.Services.GameSessions;

namespace USPSimGame.Services.PlayerSessions;

public class PlayerSessionService : IPlayerSessionService
{
    private const int MaxUserNameLength = 50;

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IGameSessionService _gameSessionService;
    private readonly IJoinAttemptLimiter _joinAttemptLimiter;

    public PlayerSessionService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IGameSessionService gameSessionService,
        IJoinAttemptLimiter joinAttemptLimiter)
    {
        _dbContextFactory = dbContextFactory;
        _gameSessionService = gameSessionService;
        _joinAttemptLimiter = joinAttemptLimiter;
    }

    public async Task<(bool Success, string? ErrorMessage, PlayerSession? PlayerSession, Team? Team, GameSession? GameSession)> JoinByCodeAsync(string joinCode, int teamId, string userName, string clientKey)
    {
        var lookup = await _gameSessionService.FindJoinableByJoinCodeAsync(joinCode, clientKey);
        if (!lookup.Success || lookup.GameSession == null)
        {
            return (false, lookup.ErrorMessage, null, null, null);
        }

        var gameSession = lookup.GameSession;

        await using var db = await _dbContextFactory.CreateDbContextAsync();

        // The team must belong to the session the code resolves to, so a player can never end up in another session.
        var team = await db.Teams.FindAsync(teamId);
        if (team == null || team.GameSessionId != gameSession.Id)
        {
            _joinAttemptLimiter.RecordFailure(clientKey);
            return (false, JoinCodeMessages.NotFound, null, null, null);
        }

        var playerSession = await AddPlayerSessionAsync(db, team, userName);

        return (true, null, playerSession, team, gameSession);
    }

    private static async Task<PlayerSession> AddPlayerSessionAsync(AppDbContext db, Team team, string userName)
    {
        var trimmedName = userName?.Trim();
        if (trimmedName?.Length > MaxUserNameLength)
        {
            trimmedName = trimmedName[..MaxUserNameLength];
        }

        var playerSession = new PlayerSession
        {
            TeamId = team.Id,
            UserName = string.IsNullOrWhiteSpace(trimmedName) ? "Player" : trimmedName,
            CreatedAt = DateTime.UtcNow,
            LastActive = DateTime.UtcNow,
            IsActive = true
        };

        db.PlayerSessions.Add(playerSession);
        await db.SaveChangesAsync();

        return playerSession;
    }
}
