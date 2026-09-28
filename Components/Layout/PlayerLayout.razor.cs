using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Layout;

public partial class PlayerLayout : LayoutComponentBase
{
    private static string Version => typeof(PlayerLayout).Assembly.GetName().Version?.ToString(3) ?? "Unknown";
}
