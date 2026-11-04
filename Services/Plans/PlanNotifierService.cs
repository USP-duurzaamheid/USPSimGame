namespace USPSimGame.Services;

public class PlanNotifierService : IPlanNotifierService
{
    private readonly ILogger<PlanNotifierService> _logger;

    public event Func<int, Task>? OnPlansChanged;
    public event Func<int, int, Task>? OnPlanLockChanged;

    public PlanNotifierService(ILogger<PlanNotifierService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyPlansChangedAsync(int gameSessionId)
    {
        if (OnPlansChanged != null)
        {
            var handlers = OnPlansChanged.GetInvocationList();
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
                    _logger.LogWarning(ex, "PlanNotifierService: Error invoking plans changed handler.");
                }
            }
        }
    }

    public async Task NotifyPlanLockChangedAsync(int planId, int gameSessionId)
    {
        if (OnPlanLockChanged != null)
        {
            var handlers = OnPlanLockChanged.GetInvocationList();
            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Func<int, int, Task> func)
                    {
                        await func.Invoke(planId, gameSessionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PlanNotifierService: Error invoking plan lock changed handler.");
                }
            }
        }
    }
}
