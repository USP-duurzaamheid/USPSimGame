using Microsoft.AspNetCore.Components.Server.Circuits;

namespace USPSimGame.Services.PlayerSessions;

// Resolving ClientConnectionInfo here forces it to be created at circuit start, while the HttpContext still exists.
public class ClientConnectionCircuitHandler : CircuitHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClientConnectionInfo _clientConnectionInfo;

    public ClientConnectionCircuitHandler(IHttpContextAccessor httpContextAccessor, ClientConnectionInfo clientConnectionInfo)
    {
        _httpContextAccessor = httpContextAccessor;
        _clientConnectionInfo = clientConnectionInfo;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _clientConnectionInfo.Refresh(_httpContextAccessor);
        return Task.CompletedTask;
    }
}
