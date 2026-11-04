namespace USPSimGame.Services.GameSessions;

public static class JoinCodeMessages
{
    // One message for unknown, malformed, finished and mismatching codes, so a caller cannot learn which condition failed.
    public const string NotFound = "No active game found with this code.";
    public const string TooManyAttempts = "Too many attempts. Please wait a minute and try again.";
}
