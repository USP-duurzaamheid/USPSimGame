using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Models;

namespace USPSimGame.Components.Game.BurgerMenu;

public enum MenuSlot
{
    None,
    Dilemmas,
    Buildings
}

public partial class InsightBurgerMenu : ComponentBase
{
    [Parameter, EditorRequired] public int GameSessionId { get; set; }
    [Parameter] public int? ScopeTeamId { get; set; }
    [Parameter] public int StartYear { get; set; } = 2026;
    [Parameter] public int CurrentMonth { get; set; }
    [Parameter] public bool ShowDilemmas { get; set; } = true;
    [Parameter] public IReadOnlyList<DilemmaModel> Dilemmas { get; set; } = [];
    [Parameter] public string? NewDilemmaCode { get; set; }
    [Parameter] public bool IsPreview { get; set; }
    [Parameter] public RenderFragment? Buildings { get; set; }
    [Parameter] public bool BuildingsEnabled { get; set; } = true;

    protected MenuSlot ActiveSlot { get; private set; } = MenuSlot.None;

    protected bool ShowOverview { get; private set; }

    protected int AttentionCount => Dilemmas.Count(d => d.NeedsAttention(CurrentMonth));

    protected override void OnParametersSet()
    {
        if (ActiveSlot == MenuSlot.Buildings && !BuildingsEnabled)
        {
            ActiveSlot = MenuSlot.None;
        }
    }

    protected void ToggleSlot(MenuSlot slot) =>
        ActiveSlot = ActiveSlot == slot ? MenuSlot.None : slot;

    public void CloseSlot()
    {
        ActiveSlot = MenuSlot.None;
        StateHasChanged();
    }

    public void OpenDilemmas()
    {
        ActiveSlot = MenuSlot.Dilemmas;
        StateHasChanged();
    }

    protected void OpenOverview() => ShowOverview = true;

    protected void CloseOverview() => ShowOverview = false;
}
