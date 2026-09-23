using Microsoft.AspNetCore.Components;
using OpenLayers.Blazor;

namespace USPSimGame.Components.Game;

public partial class GameBaseMap : ComponentBase
{
    [Parameter, EditorRequired]
    public Coordinate InitialCenter { get; set; } = default!;

    [Parameter]
    public double InitialZoom { get; set; } = 15;
}
