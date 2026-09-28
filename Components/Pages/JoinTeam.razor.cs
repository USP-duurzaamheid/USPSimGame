using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Pages;

public partial class JoinTeam : ComponentBase
{
    protected static readonly (string Initials, string Name)[] Teams =
    [
        ("HU", "Hogeschool Utrecht"),
        ("UU", "Universiteit Utrecht"),
        ("SSH", "Stichting Student Housing"),
        ("UMC", "Universitair Medisch Centrum"),
    ];

    protected string? SelectedTeam { get; set; }
}
