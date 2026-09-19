using USPSimGame.Domain.Entities;
using USPSimGame.Domain.Enums;

namespace USPSimGame.Application.Services;

public interface IGameSessionService
{
    Task<List<GameSession>> GetGameSessionsAsync();
    Task<GameSession> CreateGameSessionAsync(GameSession session);
    Task<bool> DeleteGameSessionAsync(int sessionId);
    Task UpdateGameSessionStateAsync(int sessionId, GameState newState);
    Task UpdateGameSessionStateWithTimerAsync(int sessionId, GameState newState, int monthDurationSeconds);
    Task NotifyGameStateChangedAsync(int sessionId, GameState newState);
}
