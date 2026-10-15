using System.Security.Cryptography;

namespace USPSimGame.Services.GameSessions;

public class JoinCodeGenerator : IJoinCodeGenerator
{
    public const int CodeLength = 6;
    private static readonly int Max = (int)Math.Pow(10, CodeLength);

    public string Generate() =>
        RandomNumberGenerator.GetInt32(0, Max).ToString().PadLeft(CodeLength, '0');
}
