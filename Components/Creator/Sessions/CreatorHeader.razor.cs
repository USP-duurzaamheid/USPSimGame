using Microsoft.AspNetCore.Components;
using USPSimGame.Services;
using USPSimGame.Services.Auth;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.Teams;
using USPSimGame.Services.Plans;

namespace USPSimGame.Components.Creator.Sessions;

public partial class CreatorHeader : ComponentBase, IDisposable
{
    [Inject]
    public CreatorAuthState AuthState { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        AuthState.OnStateChanged += StateHasChanged;
    }

    protected void HandleLogout()
    {
        AuthState.LogOut();
        Navigation.NavigateTo("/creator");
    }

    public void Dispose()
    {
        AuthState.OnStateChanged -= StateHasChanged;
    }
}
