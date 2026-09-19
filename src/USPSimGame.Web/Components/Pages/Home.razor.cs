using Microsoft.AspNetCore.Components;
using USPSimGame.Domain.Entities;

namespace USPSimGame.Web.Components.Pages;

public partial class Home : ComponentBase
{
    protected GameSession? SelectedSession { get; set; }

    private static string Version => typeof(Home).Assembly.GetName().Version?.ToString() ?? "Unknown";

    protected void HandleSessionSelected(GameSession session)
    {
        SelectedSession = session;
    }
}
