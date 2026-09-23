using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;

namespace USPSimGame.Components.Creator;

public partial class LoginCard : ComponentBase
{
    [Inject]
    public IAuthService AuthService { get; set; } = default!;

    [Parameter]
    public EventCallback<User> OnLoginSuccess { get; set; }

    protected string Username { get; set; } = string.Empty;
    protected string Password { get; set; } = string.Empty;
    protected string? ErrorMessage { get; set; }
    protected bool IsLoading { get; set; }

    protected async Task HandleLogin()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var user = await AuthService.AuthenticateAsync(Username, Password);
            if (user != null)
            {
                await OnLoginSuccess.InvokeAsync(user);
            }
            else
            {
                ErrorMessage = "Invalid username or password. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Authentication error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
