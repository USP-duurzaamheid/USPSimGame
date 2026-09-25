using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Auth;

public class AuthService : IAuthService
{
    public const int MinPasswordLength = 8;

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

    public async Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(string username, string email, string password)
    {
        string cleanUsername = username?.Trim() ?? string.Empty;
        string cleanEmail = email?.Trim() ?? string.Empty;
        string cleanPassword = password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanUsername) || string.IsNullOrWhiteSpace(cleanEmail) || string.IsNullOrWhiteSpace(cleanPassword))
        {
            return (false, null, "Username, email and password are required.");
        }

        if (cleanUsername.Length < 3)
        {
            return (false, null, "Username must be at least 3 characters long.");
        }

        if (!new EmailAddressAttribute().IsValid(cleanEmail))
        {
            return (false, null, "Please enter a valid email address.");
        }

        if (cleanPassword.Length < MinPasswordLength)
        {
            return (false, null, $"Password must be at least {MinPasswordLength} characters long.");
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();

        if (await db.Users.AnyAsync(u => u.Username.ToLower() == cleanUsername.ToLower()))
        {
            _logger.LogWarning("AuthService: Registration rejected, username '{Username}' is already taken.", cleanUsername);
            return (false, null, "This username is already taken.");
        }

        if (await db.Users.AnyAsync(u => u.Email.ToLower() == cleanEmail.ToLower()))
        {
            _logger.LogWarning("AuthService: Registration rejected, email '{Email}' is already in use.", cleanEmail);
            return (false, null, "An account with this email address already exists.");
        }

        var user = new User
        {
            Username = cleanUsername,
            Email = cleanEmail,
            PasswordHash = _passwordHasher.HashPassword(cleanPassword)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        _logger.LogInformation("AuthService: Registered new creator user '{Username}'.", user.Username);
        return (true, user, null);
    }
}
