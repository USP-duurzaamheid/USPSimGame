namespace USPSimGame.Services.GameSessions;

public static class JoinCodeFormat
{
    /// <summary>
    /// Allow-list validation: after trimming, the code must be exactly
    /// <see cref="JoinCodeGenerator.CodeLength"/> ASCII digits. The code stays a string so leading zeros survive.
    /// </summary>
    public static bool TryNormalize(string? input, out string code)
    {
        code = string.Empty;

        var trimmed = input?.Trim();
        if (trimmed is null || trimmed.Length != JoinCodeGenerator.CodeLength || !trimmed.All(char.IsAsciiDigit))
        {
            return false;
        }

        code = trimmed;
        return true;
    }
}
