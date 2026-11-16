using USPSimGame.Services.GameSessions;

namespace USPSimGame.Tests.Helpers;

/// <summary>
/// Stand-in for the real limiter: counts RecordFailure calls and lets the test decide whether a client is blocked.
/// </summary>
public class FakeJoinAttemptLimiter : IJoinAttemptLimiter
{
    public bool ShouldBlock { get; set; } = false;

    public int FailureCount { get; private set; } = 0;

    public void RecordFailure(string clientKey)
    {
        FailureCount++;
    }

    public bool IsBlocked(string clientKey)
    {
        return ShouldBlock;
    }
}
