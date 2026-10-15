using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Services.Layers;
using Npgsql;

namespace USPSimGame.Services.GameSessions;

public class GameSessionService : IGameSessionService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IMapLayerService _mapLayerService;
    private readonly IGameSessionNotifierService _notifier;
    private readonly ILogger<GameSessionService> _logger;

    private readonly JoinCodeAllocator _joinCodeAllocator;

    public GameSessionService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IMapLayerService mapLayerService,
        IGameSessionNotifierService notifier,
        ILogger<GameSessionService> logger,
        JoinCodeAllocator joinCodeAllocator)
    {
        _dbContextFactory = dbContextFactory;
        _mapLayerService = mapLayerService;
        _notifier = notifier;
        _logger = logger;
        _joinCodeAllocator = joinCodeAllocator;
    }

    public async Task<List<GameSession>> GetGameSessionsAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.GameSessions
            .Include(s => s.MapLayers)
                .ThenInclude(l => l.LayerDefinition)
            .Include(s => s.Teams)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<GameSession> CreateGameSessionAsync(GameSession session)
    {
        session.CreatedAt = DateTime.UtcNow;
        session.State = GameState.Setup;

        await _joinCodeAllocator.AllocateAsync(async code =>
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            session.Id = 0;
            session.JoinCode = code;
            db.GameSessions.Add(session);
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (IsJoinCodeCollision(ex))
            {
                return false;
            }
        });

        // Slow part only runs after the code is secured and the row exists
        try { await _mapLayerService.PreFetchAndSaveSessionLayersAsync(session.Id, session.CenterLatLong); }
        catch (Exception ex) { _logger.LogError(ex, "GameSessionService: Error with pre-fetching map layers for session {Id}", session.Id); }

        return session;
    }

    public async Task<GameSession?> GetByJoinCodeAsync(string joinCode)
    {
        var code = joinCode?.Trim();
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.GameSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.JoinCode == code);
    }

    private static bool IsJoinCodeCollision(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg
        && pg.ConstraintName == "IX_GameSessions_JoinCode";

    public async Task<bool> DeleteGameSessionAsync(int sessionId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var session = await db.GameSessions.FindAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        var relatedTeams = await db.Teams.Where(t => t.GameSessionId == sessionId).ToListAsync();
        if (relatedTeams.Any())
        {
            db.Teams.RemoveRange(relatedTeams);
        }

        db.GameSessions.Remove(session);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task UpdateGameSessionStateAsync(int sessionId, GameState newState)
    {
        await UpdateGameSessionStateWithTimerAsync(sessionId, newState, 120);
    }

    public async Task UpdateGameSessionStateWithTimerAsync(int sessionId, GameState newState, int monthDurationSeconds)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var session = await db.GameSessions.FindAsync(sessionId);
        if (session != null)
        {
            var oldState = session.State;
            session.State = newState;
            session.MonthDurationSeconds = monthDurationSeconds > 0 ? monthDurationSeconds : 120;

            if (newState == GameState.Play)
            {
                if (oldState == GameState.Play)
                {
                    session.TargetMonthEndUtc = DateTime.UtcNow.AddSeconds(session.MonthDurationSeconds);
                    session.RemainingSecondsOnPause = null;
                }
                else if (session.RemainingSecondsOnPause.HasValue && session.RemainingSecondsOnPause > 0)
                {
                    session.TargetMonthEndUtc = DateTime.UtcNow.AddSeconds(session.RemainingSecondsOnPause.Value);
                    session.RemainingSecondsOnPause = null;
                }
                else
                {
                    session.TargetMonthEndUtc = DateTime.UtcNow.AddSeconds(session.MonthDurationSeconds);
                }
            }
            else if (newState == GameState.Pause)
            {
                if (session.TargetMonthEndUtc.HasValue)
                {
                    int remaining = (int)(session.TargetMonthEndUtc.Value - DateTime.UtcNow).TotalSeconds;
                    session.RemainingSecondsOnPause = remaining > 0 ? remaining : 0;
                    session.TargetMonthEndUtc = null;
                }
            }
            else if (newState == GameState.Setup)
            {
                session.CurrentMonth = 0;
                session.TargetMonthEndUtc = null;
                session.RemainingSecondsOnPause = null;
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("GameSessionService: Session #{Id} state changed from {OldState} to {NewState}", sessionId, oldState, newState);

            await NotifyGameStateChangedAsync(sessionId, newState);
        }
    }

    public async Task NotifyGameStateChangedAsync(int sessionId, GameState newState)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var session = await db.GameSessions.FindAsync(sessionId);
        if (session != null)
        {
            await _notifier.NotifyGameStateChangedAsync(session);
        }
    }
}
