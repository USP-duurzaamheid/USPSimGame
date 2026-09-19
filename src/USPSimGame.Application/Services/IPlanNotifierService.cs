using USPSimGame.Domain.Entities;

namespace USPSimGame.Application.Services;

public interface IPlanNotifierService
{
    event Func<int, Task>? OnPlansChanged;
    event Func<int, int, Task>? OnPlanLockChanged;

    Task NotifyPlansChangedAsync(int gameSessionId);
    Task NotifyPlanLockChangedAsync(int planId, int gameSessionId);
}
