using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Auth;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(string username, string email, string password);
}
