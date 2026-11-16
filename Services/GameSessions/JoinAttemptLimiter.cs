using System.Collections.Concurrent;
using USPSimGame.Services.PlayerSessions;

namespace USPSimGame.Services.GameSessions;

/// <summary>
/// In-memory fixed-window limiter on FAILED join attempts per client. Successful joins are not counted (a classroom
/// shares one IP) and do not reset the counter (otherwise an attacker could interleave a known code to reset it).
/// </summary>
public class JoinAttemptLimiter : IJoinAttemptLimiter
{
    public const int MaxFailedAttempts = 10;

    // Clients whose address could not be resolved all share one key. A higher limit keeps a capture failure from
    // locking everyone out, while still capping brute force through that shared key.
    public const int MaxFailedAttemptsForUnknownClient = 100;

    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private const int PurgeThreshold = 10_000;

    private readonly TimeProvider _timeProvider;
    private readonly ILogger<JoinAttemptLimiter> _logger;
    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    public JoinAttemptLimiter(TimeProvider timeProvider, ILogger<JoinAttemptLimiter> logger)
    {
        _timeProvider = timeProvider;
        _logger = logger;
    }

    private static int GetLimit(string clientKey) =>
        clientKey == ClientConnectionInfo.UnknownClient ? MaxFailedAttemptsForUnknownClient : MaxFailedAttempts;

    public bool IsBlocked(string clientKey)
    {
        if (!_entries.TryGetValue(clientKey, out var entry))
        {
            return false;
        }

        var now = _timeProvider.GetUtcNow();
        lock (entry)
        {
            return now - entry.WindowStart < Window && entry.FailedAttempts >= GetLimit(clientKey);
        }
    }

    public void RecordFailure(string clientKey)
    {
        if (clientKey == ClientConnectionInfo.UnknownClient)
        {
            _logger.LogWarning("JoinAttemptLimiter: Failed join attempt from a client whose address could not be resolved; using the shared fallback limit.");
        }

        var now = _timeProvider.GetUtcNow();
        var entry = _entries.GetOrAdd(clientKey, _ => new Entry { WindowStart = now });

        lock (entry)
        {
            if (now - entry.WindowStart >= Window)
            {
                entry.WindowStart = now;
                entry.FailedAttempts = 0;
            }

            entry.FailedAttempts++;
        }

        if (_entries.Count > PurgeThreshold)
        {
            PurgeExpired(now);
        }
    }

    private void PurgeExpired(DateTimeOffset now)
    {
        foreach (var (key, entry) in _entries)
        {
            bool expired;
            lock (entry)
            {
                expired = now - entry.WindowStart >= Window;
            }

            if (expired)
            {
                _entries.TryRemove(key, out _);
            }
        }
    }

    private sealed class Entry
    {
        public int FailedAttempts;
        public DateTimeOffset WindowStart;
    }
}
