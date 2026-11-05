using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Models;

namespace USPSimGame.Components.Game.Fines;

/// <summary>
/// Message at the top of the game screen when a fine starts or stops.
/// </summary>
public partial class FineToast : ComponentBase
{
    [Parameter, EditorRequired] public FineModel Fine { get; set; } = default!;

    [Parameter] public EventCallback OnClose { get; set; }
}
