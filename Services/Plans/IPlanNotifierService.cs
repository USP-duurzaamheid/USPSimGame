using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public interface IPlanNotifierService
{
    event Func<int, Task>? OnPlansChanged;
    event Func<int, int, Task>? OnPlanLockChanged;

    Task NotifyPlansChangedAsync(int gameSessionId);
    Task NotifyPlanLockChangedAsync(int planId, int gameSessionId);
}
