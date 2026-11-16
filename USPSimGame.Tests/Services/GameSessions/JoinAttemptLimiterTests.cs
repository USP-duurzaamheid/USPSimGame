using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.PlayerSessions;

namespace USPSimGame.Tests.Services.GameSessions;

public class JoinAttemptLimiterTests
{
    private const string ClientA = "10.0.0.1";
    private const string ClientB = "10.0.0.2";

    // The fake clock only moves when the test calls Advance, so no Thread.Sleep / Task.Delay is ever needed.
    private readonly FakeTimeProvider _time = new();
    private readonly JoinAttemptLimiter _limiter;

    public JoinAttemptLimiterTests()
    {
        _limiter = new JoinAttemptLimiter(_time, NullLogger<JoinAttemptLimiter>.Instance);
    }

    private void Fail(string clientKey, int times)
    {
        for (var i = 0; i < times; i++)
        {
            _limiter.RecordFailure(clientKey);
        }
    }

    [Fact]
    public void IsBlocked_NewClient_IsFalse()
    {
        //Act
        var blocked = _limiter.IsBlocked(ClientA);

        //Assert
        Assert.False(blocked);
    }

    [Fact]
    public void IsBlocked_OneBelowLimit_IsFalse()
    {
        //Arrange
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts - 1);

        //Act & Assert
        Assert.False(_limiter.IsBlocked(ClientA));
    }

    [Fact]
    public void IsBlocked_AtLimit_IsTrue()
    {
        //Arrange
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts);

        //Act & Assert
        Assert.True(_limiter.IsBlocked(ClientA));
    }

    [Fact]
    public void IsBlocked_AfterWindowPassed_IsFalse()
    {
        //Arrange
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts);

        //Act
        _time.Advance(JoinAttemptLimiter.Window + TimeSpan.FromSeconds(1));

        //Assert
        Assert.False(_limiter.IsBlocked(ClientA));
    }

    [Fact]
    public void IsBlocked_ExactlyAtWindowEnd_IsFalse()
    {
        //Arrange
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts);

        //Act
        _time.Advance(JoinAttemptLimiter.Window);

        //Assert (the limiter uses ">= Window", so at exactly 1 minute the block is lifted)
        Assert.False(_limiter.IsBlocked(ClientA));
    }

    [Fact]
    public void IsBlocked_JustBeforeWindowEnd_IsTrue()
    {
        //Arrange
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts);

        //Act
        _time.Advance(JoinAttemptLimiter.Window - TimeSpan.FromMilliseconds(1));

        //Assert
        Assert.True(_limiter.IsBlocked(ClientA));
    }

    [Fact]
    public void RecordFailure_AfterWindow_StartsCountingFromOne()
    {
        //Arrange
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts);
        _time.Advance(JoinAttemptLimiter.Window);

        //Act: if the counter did not reset, this would be failure 11 and the client would be blocked again.
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts - 1);

        //Assert
        Assert.False(_limiter.IsBlocked(ClientA));

        // One more reaches the limit again, which proves the count really restarted at 1 (and not at 0 or 11).
        Fail(ClientA, 1);
        Assert.True(_limiter.IsBlocked(ClientA));
    }

    [Fact]
    public void IsBlocked_BlockingClientA_DoesNotBlockClientB()
    {
        //Arrange
        Fail(ClientA, JoinAttemptLimiter.MaxFailedAttempts);

        //Act & Assert
        Assert.True(_limiter.IsBlocked(ClientA));
        Assert.False(_limiter.IsBlocked(ClientB));
    }

    [Fact]
    public void IsBlocked_UnknownClient_UsesHigherLimit()
    {
        //Arrange
        const string unknown = ClientConnectionInfo.UnknownClient;

        //Act & Assert
        Fail(unknown, JoinAttemptLimiter.MaxFailedAttempts);
        Assert.False(_limiter.IsBlocked(unknown));

        Fail(unknown, JoinAttemptLimiter.MaxFailedAttemptsForUnknownClient - JoinAttemptLimiter.MaxFailedAttempts - 1);
        Assert.False(_limiter.IsBlocked(unknown));

        Fail(unknown, 1);
        Assert.True(_limiter.IsBlocked(unknown));
    }
}
