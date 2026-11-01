namespace USPSimGame.Services.PlayerSessions;

/// <summary>
/// Identifies the client of the current circuit (used as the rate limiting key). In an interactive Blazor circuit the
/// HttpContext is only available while the circuit starts, so the address is captured then and kept for the circuit.
/// </summary>
public class ClientConnectionInfo
{
    public const string UnknownClient = "unknown";

    public string ClientKey { get; private set; }

    public ClientConnectionInfo(IHttpContextAccessor httpContextAccessor)
    {
        ClientKey = Resolve(httpContextAccessor) ?? UnknownClient;
    }

    public void Refresh(IHttpContextAccessor httpContextAccessor)
    {
        if (ClientKey == UnknownClient)
        {
            ClientKey = Resolve(httpContextAccessor) ?? UnknownClient;
        }
    }

    private static string? Resolve(IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
