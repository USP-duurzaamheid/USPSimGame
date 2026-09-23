using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public class PlayerSessionService : IPlayerSessionService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IPasswordHasher _passwordHasher;

    public PlayerSessionService(IDbContextFactory<AppDbContext> dbContextFactory, IPasswordHasher passwordHasher)
    {
        _dbContextFactory = dbContextFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool Success, string? ErrorMessage, PlayerSession? PlayerSession, Team? Team, GameSession? GameSession)> ConnectAsync(int teamId, string userName, string password)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var team = await db.Teams.FindAsync(teamId);
        if (team == null)
        {
            return (false, "Selected team was not found.", null, null, null);
        }

        var gameSession = await db.GameSessions.FindAsync(team.GameSessionId);
        if (gameSession == null)
        {
            return (false, "Associated game session was not found.", null, null, null);
        }

        // Validate password
        bool isPasswordValid = _passwordHasher.VerifyPassword(team.PasswordHash, password);
        if (!isPasswordValid)
        {
            return (false, "Invalid team password.", null, null, null);
        }

        // Create new PlayerSession
        var playerSession = new PlayerSession
        {
            TeamId = team.Id,
            UserName = string.IsNullOrWhiteSpace(userName) ? "Player" : userName.Trim(),
            CreatedAt = DateTime.UtcNow,
            LastActive = DateTime.UtcNow,
            IsActive = true
        };

        db.PlayerSessions.Add(playerSession);
        await db.SaveChangesAsync();

        return (true, null, playerSession, team, gameSession);
    }
}
