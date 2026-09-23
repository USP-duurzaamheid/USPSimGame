using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IDbContextFactory<AppDbContext> dbContextFactory, IPasswordHasher passwordHasher, ILogger<AuthService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("AuthService: Empty username or password provided.");
            return null;
        }

        string cleanUsername = username.Trim();
        string cleanPassword = password.Trim();

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == cleanUsername.ToLower());

        if (user == null)
        {
            _logger.LogWarning("AuthService: User '{Username}' not found in database.", cleanUsername);
            return null;
        }

        bool isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHash, cleanPassword);

        if (isPasswordValid)
        {
            _logger.LogInformation("AuthService: Authentication successful for user '{Username}'.", user.Username);
            return user;
        }

        _logger.LogWarning("AuthService: Password verification failed for user '{Username}'.", user.Username);
        return null;
    }
}
