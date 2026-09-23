using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public class GameSessionNotifierService : IGameSessionNotifierService
{
    private readonly ILogger<GameSessionNotifierService> _logger;

    public event Func<GameSession, Task>? OnGameSessionStateChanged;

    public GameSessionNotifierService(ILogger<GameSessionNotifierService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyGameStateChangedAsync(GameSession session)
    {
        if (OnGameSessionStateChanged != null)
        {
            var handlers = OnGameSessionStateChanged.GetInvocationList();
            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Func<GameSession, Task> func)
                    {
                        await func.Invoke(session);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GameSessionNotifierService: Error invoking state change handler.");
                }
            }
        }
    }
}
