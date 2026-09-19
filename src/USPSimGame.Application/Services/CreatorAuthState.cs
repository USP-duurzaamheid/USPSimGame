using USPSimGame.Domain.Entities;

namespace USPSimGame.Application.Services;

public class CreatorAuthState
{
    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser != null;

    public event Action? OnStateChanged;

    public void LogIn(User user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void LogOut()
    {
        CurrentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
