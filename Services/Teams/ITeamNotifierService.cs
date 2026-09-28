namespace USPSimGame.Services.Teams;

public interface ITeamNotifierService
{
    event Func<int, Task>? OnTeamAreaChanged;
    Task NotifyTeamAreaChangedAsync(int gameSessionId);
}
