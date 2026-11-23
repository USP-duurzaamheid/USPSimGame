using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Pages;

public partial class Home : ComponentBase
{
    private static string Version => typeof(Home).Assembly.GetName().Version?.ToString() ?? "Unknown";
}
