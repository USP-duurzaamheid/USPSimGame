using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Services.GameSessions;

namespace USPSimGame.Tests.Helpers;

/// <summary>
/// Only FindJoinableByJoinCodeAsync has behaviour: it returns whatever the test put in <see cref="LookupResult"/>.
/// </summary>
public class FakeGameSessionService : IGameSessionService
{
    public (bool Success, string? ErrorMessage, GameSession? GameSession) LookupResult { get; set; }

    public Task<(bool Success, string? ErrorMessage, GameSession? GameSession)> FindJoinableByJoinCodeAsync(string joinCode, string clientKey)
        => Task.FromResult(LookupResult);

    public Task<List<GameSession>> GetGameSessionsAsync() => throw new NotImplementedException();
    public Task<GameSession> CreateGameSessionAsync(GameSession session) => throw new NotImplementedException();
    public Task<GameSession?> GetByJoinCodeAsync(string joinCode) => throw new NotImplementedException();
    public Task<bool> DeleteGameSessionAsync(int sessionId) => throw new NotImplementedException();
    public Task UpdateGameSessionStateAsync(int sessionId, GameState newState) => throw new NotImplementedException();
    public Task UpdateGameSessionStateWithTimerAsync(int sessionId, GameState newState, int monthDurationSeconds) => throw new NotImplementedException();
    public Task NotifyGameStateChangedAsync(int sessionId, GameState newState) => throw new NotImplementedException();
}
