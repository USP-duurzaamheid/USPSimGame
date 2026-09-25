using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services.Auth;

namespace USPSimGame.Components.Creator.Auth;

public partial class RegisterCard : ComponentBase
{
    [Inject]
    public IAuthService AuthService { get; set; } = default!;

    [Parameter]
    public EventCallback<User> OnRegisterSuccess { get; set; }

    [Parameter]
    public EventCallback OnShowLogin { get; set; }

    protected int MinPasswordLength => USPSimGame.Services.Auth.AuthService.MinPasswordLength;

    protected string Username { get; set; } = string.Empty;
    protected string Email { get; set; } = string.Empty;
    protected string Password { get; set; } = string.Empty;
    protected string ConfirmPassword { get; set; } = string.Empty;
    protected string? ErrorMessage { get; set; }
    protected bool IsLoading { get; set; }

    protected async Task HandleRegister()
    {
        ErrorMessage = null;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsLoading = true;

        try
        {
            var (success, user, error) = await AuthService.RegisterAsync(Username, Email, Password);
            if (success && user != null)
            {
                await OnRegisterSuccess.InvokeAsync(user);
            }
            else
            {
                ErrorMessage = error ?? "Registration failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
