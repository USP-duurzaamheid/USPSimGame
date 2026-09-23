namespace USPSimGame.Services;

public class TeamNotifierService : ITeamNotifierService
{
    private readonly ILogger<TeamNotifierService> _logger;

    public event Func<int, Task>? OnTeamAreaChanged;

    public TeamNotifierService(ILogger<TeamNotifierService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyTeamAreaChangedAsync(int gameSessionId)
    {
        if (OnTeamAreaChanged != null)
        {
            var handlers = OnTeamAreaChanged.GetInvocationList();
            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Func<int, Task> func)
                    {
                        await func.Invoke(gameSessionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "TeamNotifierService: Error invoking team area change handler.");
                }
            }
        }
    }
}
