using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Auth;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password);
}
