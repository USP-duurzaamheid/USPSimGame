using USPSimGame.Domain.Entities;

namespace USPSimGame.Application.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password);
}
