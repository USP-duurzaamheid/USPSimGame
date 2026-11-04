using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Services.GameSessions;
using USPSimGame.Tests.Helpers;

namespace USPSimGame.Tests.Services.GameSessions;

public class GameSessionServiceTests
{
    private const string Client = "10.0.0.1";

    private readonly TestDbContextFactory _dbFactory = new();
    private readonly FakeJoinAttemptLimiter _limiter = new();
    private readonly GameSessionService _service;

    public GameSessionServiceTests()
    {
        // IMapLayerService and the notifier are passed as null!: the join methods never touch them.
        // If a test here ever throws a NullReferenceException, the method under test started using one of them.
        _service = new GameSessionService(
            _dbFactory,
            null!,
            null!,
            NullLogger<GameSessionService>.Instance,
            new JoinCodeAllocator(new JoinCodeGenerator()),
            _limiter);
    }

    private async Task<GameSession> SeedSessionAsync(string joinCode, GameState state, string name = "Session")
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var session = new GameSession { Name = name, JoinCode = joinCode, State = state };
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    [Theory]
    [InlineData(GameState.Setup)]
    [InlineData(GameState.Pause)]
    [InlineData(GameState.Play)]
    public async Task FindJoinable_ValidCode_JoinableState_ReturnsSession(GameState state)
    {
        //Arrange
        var seeded = await SeedSessionAsync("123456", state);

        //Act
        var result = await _service.FindJoinableByJoinCodeAsync("123456", Client);

        //Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.GameSession);
        Assert.Equal(seeded.Id, result.GameSession.Id);
        Assert.Equal(0, _limiter.FailureCount);
    }

    [Theory]
    [InlineData(GameState.Simulation)]
    [InlineData(GameState.Complete)]
    public async Task FindJoinable_NonJoinableState_ReturnsNotFound_AndRecordsFailure(GameState state)
    {
        //Arrange
        await SeedSessionAsync("123456", state);

        //Act
        var result = await _service.FindJoinableByJoinCodeAsync("123456", Client);

        //Assert
        Assert.False(result.Success);
        Assert.Equal(JoinCodeMessages.NotFound, result.ErrorMessage);
        Assert.Null(result.GameSession);
        Assert.Equal(1, _limiter.FailureCount);
    }

    [Fact]
    public async Task FindJoinable_UnknownCode_ReturnsNotFound_AndRecordsFailure()
    {
        //Arrange
        await SeedSessionAsync("123456", GameState.Setup);

        //Act
        var result = await _service.FindJoinableByJoinCodeAsync("654321", Client);

        //Assert
        Assert.False(result.Success);
        Assert.Equal(JoinCodeMessages.NotFound, result.ErrorMessage);
        Assert.Null(result.GameSession);
        Assert.Equal(1, _limiter.FailureCount);
    }

    [Theory]
    [InlineData("12 456")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("1234567")]
    public async Task FindJoinable_MalformedCode_ReturnsNotFound_AndRecordsFailure(string joinCode)
    {
        //Arrange
        await SeedSessionAsync("123456", GameState.Setup);

        //Act
        var result = await _service.FindJoinableByJoinCodeAsync(joinCode, Client);

        //Assert
        Assert.False(result.Success);
        Assert.Equal(JoinCodeMessages.NotFound, result.ErrorMessage);
        Assert.Equal(1, _limiter.FailureCount);
    }

    [Fact]
    public async Task FindJoinable_CodeWithLeadingZeros_IsFound()
    {
        //Arrange
        var seeded = await SeedSessionAsync("000042", GameState.Setup);

        //Act
        var result = await _service.FindJoinableByJoinCodeAsync("000042", Client);

        //Assert
        Assert.True(result.Success);
        Assert.Equal(seeded.Id, result.GameSession!.Id);
    }

    [Fact]
    public async Task FindJoinable_TwoSessions_ReturnsTheMatchingOneOnly()
    {
        //Arrange
        var sessionA = await SeedSessionAsync("111111", GameState.Setup, "A");
        var sessionB = await SeedSessionAsync("222222", GameState.Setup, "B");

        //Act
        var result = await _service.FindJoinableByJoinCodeAsync("222222", Client);

        //Assert
        Assert.True(result.Success);
        Assert.Equal(sessionB.Id, result.GameSession!.Id);
        Assert.NotEqual(sessionA.Id, result.GameSession.Id);
    }

    [Fact]
    public async Task FindJoinable_BlockedClient_ReturnsTooManyAttempts_EvenForValidCode_AndDoesNotRecordFailure()
    {
        //Arrange
        await SeedSessionAsync("123456", GameState.Setup);
        _limiter.ShouldBlock = true;

        //Act
        var result = await _service.FindJoinableByJoinCodeAsync("123456", Client);

        //Assert
        Assert.False(result.Success);
        Assert.Equal(JoinCodeMessages.TooManyAttempts, result.ErrorMessage);
        Assert.Null(result.GameSession);
        Assert.Equal(0, _limiter.FailureCount);
    }

    [Fact]
    public async Task FindJoinable_UnknownAndFinishedCode_GiveIdenticalMessage()
    {
        //Arrange
        await SeedSessionAsync("123456", GameState.Complete);

        //Act
        var finished = await _service.FindJoinableByJoinCodeAsync("123456", Client);
        var unknown = await _service.FindJoinableByJoinCodeAsync("999999", Client);

        //Assert: a caller must not be able to tell "exists but finished" apart from "does not exist" (OWASP).
        Assert.False(finished.Success);
        Assert.False(unknown.Success);
        Assert.Equal(unknown.ErrorMessage, finished.ErrorMessage);
    }

    [Fact]
    public async Task FindJoinable_Lookup_TakesUnderOneSecond()
    {
        //Arrange
        await SeedSessionAsync("123456", GameState.Setup);

        //Act
        var stopwatch = Stopwatch.StartNew();
        await _service.FindJoinableByJoinCodeAsync("123456", Client);
        stopwatch.Stop();

        //Assert: this only proves the code adds no slow logic; InMemory says nothing about real Postgres speed.
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Lookup took {stopwatch.ElapsedMilliseconds} ms.");
    }
}
