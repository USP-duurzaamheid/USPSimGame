using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public class PlayerSessionState
{
    public PlayerSession? CurrentPlayerSession { get; private set; }
    public Team? CurrentTeam { get; private set; }
    public GameSession? CurrentGameSession { get; private set; }
    public User? CurrentAdminUser { get; private set; }
    public bool IsAdmin { get; private set; }

    public bool IsConnected => (CurrentPlayerSession != null && CurrentPlayerSession.IsActive) || IsAdmin;

    public event Action? OnStateChanged;

    public void SetSession(PlayerSession playerSession, Team team, GameSession gameSession)
    {
        CurrentPlayerSession = playerSession;
        CurrentTeam = team;
        CurrentGameSession = gameSession;
        CurrentAdminUser = null;
        IsAdmin = false;
        NotifyStateChanged();
    }

    public void SetAdminSession(User adminUser, GameSession gameSession)
    {
        CurrentAdminUser = adminUser;
        IsAdmin = true;
        CurrentGameSession = gameSession;
        CurrentTeam = null;
        CurrentPlayerSession = new PlayerSession
        {
            Id = -adminUser.Id,
            UserName = adminUser.Username,
            TeamId = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastActive = DateTime.UtcNow
        };
        NotifyStateChanged();
    }

    public void ClearSession()
    {
        CurrentPlayerSession = null;
        CurrentTeam = null;
        CurrentGameSession = null;
        CurrentAdminUser = null;
        IsAdmin = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
