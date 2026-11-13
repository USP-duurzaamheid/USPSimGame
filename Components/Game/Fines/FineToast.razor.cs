using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Models;

namespace USPSimGame.Components.Game.Fines;

public partial class FineToast : ComponentBase
{
    [Parameter, EditorRequired] public FineModel Fine { get; set; } = default!;
    [Parameter] public EventCallback OnClose { get; set; }
}
