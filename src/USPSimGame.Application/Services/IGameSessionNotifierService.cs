using USPSimGame.Domain.Entities;

namespace USPSimGame.Application.Services;

public interface IGameSessionNotifierService
{
    event Func<GameSession, Task>? OnGameSessionStateChanged;
    Task NotifyGameStateChangedAsync(GameSession session);
}
