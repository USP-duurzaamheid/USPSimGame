using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public interface IGameSessionNotifierService
{
    event Func<GameSession, Task>? OnGameSessionStateChanged;
    Task NotifyGameStateChangedAsync(GameSession session);
}
