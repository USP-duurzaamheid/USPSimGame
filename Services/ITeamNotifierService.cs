namespace USPSimGame.Services;

public interface ITeamNotifierService
{
    event Func<int, Task>? OnTeamAreaChanged;
    Task NotifyTeamAreaChangedAsync(int gameSessionId);
}
