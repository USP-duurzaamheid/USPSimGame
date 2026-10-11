namespace USPSimGame.Services.GameSessions;
public class JoinCodeAllocator
{
    public const int MaxAttempts = 5;
    private readonly IJoinCodeGenerator _generator;

    public JoinCodeAllocator(IJoinCodeGenerator generator) => _generator = generator;

    public async Task<string> AllocateAsync(Func<string, Task<bool>> tryReserve)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var code = _generator.Generate();
            if (await tryReserve(code)) return code;
        }
        throw new InvalidOperationException(
            $"Could not allocate a unique join code after {MaxAttempts} attempts.");
    }
}