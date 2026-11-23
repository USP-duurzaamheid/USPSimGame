namespace USPSimGame.Services.GameSessions;

public interface IJoinAttemptLimiter
{
    bool IsBlocked(string clientKey);
    void RecordFailure(string clientKey);
}
