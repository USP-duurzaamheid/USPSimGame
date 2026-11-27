using USPSimGame.Data.Entities;

namespace USPSimGame.Services.PlayerSessions;

public class PlayerSessionState
{
    public PlayerSession? CurrentPlayerSession { get; private set; }
    public Team? CurrentTeam { get; private set; }
    public GameSession? CurrentGameSession { get; private set; }

    // Name of the logged-in creator (ASP.NET Identity user name) when the game is opened as admin.
    public string? AdminUserName { get; private set; }
    public bool IsAdmin { get; private set; }

    public bool IsConnected => (CurrentPlayerSession != null && CurrentPlayerSession.IsActive) || IsAdmin;

    public event Action? OnStateChanged;

    public void SetSession(PlayerSession playerSession, Team team, GameSession gameSession)
    {
        CurrentPlayerSession = playerSession;
        CurrentTeam = team;
        CurrentGameSession = gameSession;
        AdminUserName = null;
        IsAdmin = false;
        NotifyStateChanged();
    }

    // An admin has no PlayerSession: that is a database row for a player in a team.
    // IsConnected already treats IsAdmin as connected.
    public void SetAdminSession(string adminUserName, GameSession gameSession)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adminUserName);
        ArgumentNullException.ThrowIfNull(gameSession);

        AdminUserName = adminUserName;
        IsAdmin = true;
        CurrentGameSession = gameSession;
        CurrentTeam = null;
        CurrentPlayerSession = null;
        NotifyStateChanged();
    }

    public void ClearSession()
    {
        CurrentPlayerSession = null;
        CurrentTeam = null;
        CurrentGameSession = null;
        AdminUserName = null;
        IsAdmin = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
