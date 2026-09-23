using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Services.Layers;

namespace USPSimGame.Services;

public class GameSessionService : IGameSessionService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IMapLayerService _mapLayerService;
    private readonly IGameSessionNotifierService _notifier;
    private readonly ILogger<GameSessionService> _logger;

    public GameSessionService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IMapLayerService mapLayerService,
        IGameSessionNotifierService notifier,
        ILogger<GameSessionService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapLayerService = mapLayerService;
        _notifier = notifier;
        _logger = logger;
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
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        session.CreatedAt = DateTime.UtcNow;
        session.State = GameState.Setup;
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();

        _logger.LogInformation("GameSessionService: Created GameSession '{Name}' (Id: {Id}). Triggering MapLayer pre-fetch...", session.Name, session.Id);

        try
        {
            await _mapLayerService.PreFetchAndSaveSessionLayersAsync(session.Id, session.CenterLatLong);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GameSessionService: Error pre-fetching map layers for session {Id}", session.Id);
        }

        return session;
    }

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
