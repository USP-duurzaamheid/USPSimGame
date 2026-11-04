using Microsoft.EntityFrameworkCore;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Tests.Helpers;

namespace USPSimGame.Tests.Services.PlayerSessions;

public class PlayerSessionServiceTests
{
    private const string Client = "10.0.0.1";
    private const string Code = "123456";

    private readonly TestDbContextFactory _dbFactory = new();
    private readonly FakeGameSessionService _gameSessions = new();
    private readonly FakeJoinAttemptLimiter _limiter = new();
    private readonly PlayerSessionService _service;

    public PlayerSessionServiceTests()
    {
        _service = new PlayerSessionService(_dbFactory, _gameSessions, _limiter);
    }

    // Seeds two sessions, each with one team, and makes the fake lookup resolve the join code to session 1.
    private async Task<(GameSession Session, Team Team, Team OtherSessionTeam)> SeedAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var session = new GameSession { Name = "Mine", JoinCode = Code, State = GameState.Setup };
        var otherSession = new GameSession { Name = "Other", JoinCode = "999999", State = GameState.Setup };
        db.GameSessions.AddRange(session, otherSession);
        await db.SaveChangesAsync();

        var team = new Team { Name = "UU", Color = "#ffcd00", GameSessionId = session.Id };
        var otherTeam = new Team { Name = "HU", Color = "#e6302b", GameSessionId = otherSession.Id };
        db.Teams.AddRange(team, otherTeam);
        await db.SaveChangesAsync();

        _gameSessions.LookupResult = (true, null, session);
        return (session, team, otherTeam);
    }

    private async Task<List<PlayerSession>> SavedPlayerSessionsAsync()
    {
        // Fresh context: proves the row really was saved, not just that the returned object looks right.
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PlayerSessions.AsNoTracking().ToListAsync();
    }

    [Fact]
    public async Task JoinByCode_TeamInSession_Succeeds_AndSavesPlayerSession()
    {
        //Arrange
        var (session, team, _) = await SeedAsync();

        //Act
        var result = await _service.JoinByCodeAsync(Code, team.Id, "Ann", Client);

        //Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(team.Id, result.Team!.Id);
        Assert.Equal(session.Id, result.GameSession!.Id);

        var saved = Assert.Single(await SavedPlayerSessionsAsync());
        Assert.Equal(result.PlayerSession!.Id, saved.Id);
        Assert.Equal(team.Id, saved.TeamId);
        Assert.Equal("Ann", saved.UserName);
        Assert.True(saved.IsActive);
        Assert.Equal(0, _limiter.FailureCount);
    }

    [Theory]
    [InlineData(JoinCodeMessages.NotFound)]
    [InlineData(JoinCodeMessages.TooManyAttempts)]
    public async Task JoinByCode_LookupFails_PassesMessageThrough_SavesNothing_AndDoesNotCountAgain(string message)
    {
        //Arrange
        var (_, team, _) = await SeedAsync();
        _gameSessions.LookupResult = (false, message, null);

        //Act
        var result = await _service.JoinByCodeAsync(Code, team.Id, "Ann", Client);

        //Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.ErrorMessage);
        Assert.Null(result.PlayerSession);
        Assert.Empty(await SavedPlayerSessionsAsync());
        // The lookup already counted this failure itself; counting here as well would double count.
        Assert.Equal(0, _limiter.FailureCount);
    }

    [Fact]
    public async Task JoinByCode_TeamDoesNotExist_ReturnsNotFound_RecordsFailure_SavesNothing()
    {
        //Arrange
        await SeedAsync();

        //Act
        var result = await _service.JoinByCodeAsync(Code, teamId: 12345, "Ann", Client);

        //Assert
        Assert.False(result.Success);
        Assert.Equal(JoinCodeMessages.NotFound, result.ErrorMessage);
        Assert.Equal(1, _limiter.FailureCount);
        Assert.Empty(await SavedPlayerSessionsAsync());
    }

    [Fact]
    public async Task JoinByCode_TeamFromOtherSession_ReturnsNotFound_RecordsFailure_SavesNothing()
    {
        //Arrange: valid code for session 1, but the team id belongs to session 2.
        var (_, _, otherSessionTeam) = await SeedAsync();

        //Act
        var result = await _service.JoinByCodeAsync(Code, otherSessionTeam.Id, "Ann", Client);

        //Assert
        Assert.False(result.Success);
        Assert.Equal(JoinCodeMessages.NotFound, result.ErrorMessage);
        Assert.Null(result.Team);
        Assert.Null(result.GameSession);
        Assert.Equal(1, _limiter.FailureCount);
        Assert.Empty(await SavedPlayerSessionsAsync());
    }

    public static TheoryData<string, string> UserNames => new()
    {
        { "", "Player" },
        { " ", "Player" },
        { " Ann ", "Ann" },
        { new string('a', 60), new string('a', 50) },
        { new string('a', 50), new string('a', 50) },
        { new string('a', 51), new string('a', 50) },
    };

    [Theory]
    [MemberData(nameof(UserNames))]
    public async Task JoinByCode_UserName_IsNormalized(string input, string expected)
    {
        //Arrange
        var (_, team, _) = await SeedAsync();

        //Act
        var result = await _service.JoinByCodeAsync(Code, team.Id, input, Client);

        //Assert
        Assert.True(result.Success);
        var saved = Assert.Single(await SavedPlayerSessionsAsync());
        Assert.Equal(expected, saved.UserName);
    }
}
