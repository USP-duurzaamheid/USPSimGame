using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;
using USPSimGame.Services.Auth;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.Teams;
using USPSimGame.Services.Plans;

namespace USPSimGame.Components.Pages.Creator;

public partial class Creator : ComponentBase, IDisposable
{
    [Inject]
    public CreatorAuthState AuthState { get; set; } = default!;

    protected override void OnInitialized()
    {
        AuthState.OnStateChanged += StateHasChanged;
    }

    protected void HandleLoginSuccess(User user)
    {
        AuthState.LogIn(user);
    }

    protected void HandleLogout()
    {
        AuthState.LogOut();
    }

    public void Dispose()
    {
        AuthState.OnStateChanged -= StateHasChanged;
    }
}
