using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password);
}
