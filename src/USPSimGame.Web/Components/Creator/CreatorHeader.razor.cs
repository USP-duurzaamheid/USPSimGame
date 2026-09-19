using Microsoft.AspNetCore.Components;
using USPSimGame.Application.Services;

namespace USPSimGame.Web.Components.Creator;

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
