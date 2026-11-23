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

    public async Task<(bool Success, string? ErrorMessage, Team? Team, GameSession? GameSession)> CheckLobby(string joinCode, int teamId, string clientKey)
    {
        var lookup = await _gameSessionService.FindJoinableByJoinCodeAsync(joinCode, clientKey); // keeps track of
        if (!lookup.Success || lookup.GameSession == null)
        {
            return (false, lookup.ErrorMessage, null, null);
        }

        var gameSession = lookup.GameSession;

        await using var db = await _dbContextFactory.CreateDbContextAsync();

        // The team must belong to the session the code resolves to, so a player can never end up in another session.
        var team = await db.Teams.FindAsync(teamId);
        if (team == null || team.GameSessionId != gameSession.Id)
        {
            _joinAttemptLimiter.RecordFailure(clientKey);
            return (false, JoinCodeMessages.NotFound, null, null);
        }



        return (true, null, team, gameSession);
    }

    public static async Task<(bool Success, PlayerSession? Player)> AddPlayerSessionAsync(AppDbContext context, int team, string userName, string joinCode)
    {
        int GameId = await context.GameSessions.Where(id => id.JoinCode == joinCode).Select(o => o.Id).SingleAsync();
        var GameFull = await GameSessionFull(context, GameId);
        if (GameFull) return (false, null);

        var trimmedName = userName?.Trim();
        if (trimmedName?.Length > MaxUserNameLength)
        {
            trimmedName = trimmedName[..MaxUserNameLength];
        }

        var playerSession = new PlayerSession
        {
            Id = Guid.NewGuid().ToString(),
            TeamId = team,
            UserName = string.IsNullOrWhiteSpace(trimmedName) ? "Player" : trimmedName,
            CreatedAt = DateTime.UtcNow,
            LastActive = DateTime.UtcNow,
            IsActive = true,
            GameId = GameId
        };

        context.PlayerSessions.Add(playerSession);
        await context.SaveChangesAsync();

        return (true, playerSession);
    }


    /// <summary>
    /// Check if a lobby is full, max players is 50
    /// </summary>
    /// <param name="context"></param>
    /// <param name="gameId"></param>
    /// <returns></returns>
    public static async Task<bool> GameSessionFull(AppDbContext context, int gameId)
    {
        try
        {
            int SessionPlayerTotal = context.PlayerSessions.Select(p => p.GameId).Where(id => id == gameId).ToList().Count();
            if (SessionPlayerTotal >= 50) return true;
        }
        catch (Exception)
        {
            return false;
        }
        return false;
    }
}
