using Microsoft.AspNetCore.Components;
using USPSimGame.Domain.Entities;
using USPSimGame.Application.Services;

namespace USPSimGame.Web.Components.Pages.Creator;

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
