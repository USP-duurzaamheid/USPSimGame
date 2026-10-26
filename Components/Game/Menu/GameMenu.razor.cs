using Microsoft.AspNetCore.Components;
using USPSimGame.Components.Game.Dilemmas;

namespace USPSimGame.Components.Game.Menu;

public enum MenuSlot
{
    None,
    Dilemmas,
    Buildings
}

/// <summary>
/// Game menu in the bottom-left corner (game layout design): Overzicht opens the KPI charts,
/// Doelen the active dilemmas and BUILDINGS the development plans. Doelen and BUILDINGS
/// open a panel next to the menu; only one of them is open at a time.
/// </summary>
public partial class GameMenu : ComponentBase
{
    [Parameter, EditorRequired] public int GameSessionId { get; set; }

    /// <summary>Team whose KPI charts are shown; null shows all teams (admin).</summary>
    [Parameter] public int? ScopeTeamId { get; set; }

    [Parameter] public int StartYear { get; set; } = 2026;

    /// <summary>Current game month of the session (0 = first month).</summary>
    [Parameter] public int CurrentMonth { get; set; }

    /// <summary>Dilemmas are team assignments, so the Doelen button is hidden for admins.</summary>
    [Parameter] public bool ShowDilemmas { get; set; } = true;

    [Parameter] public IReadOnlyList<DilemmaViewModel> Dilemmas { get; set; } = [];

    /// <summary>Marks the dilemmas as example data that is not yet provided by the backend.</summary>
    [Parameter] public bool IsPreview { get; set; }

    /// <summary>Content of the BUILDINGS panel (the development plans panel).</summary>
    [Parameter] public RenderFragment? Buildings { get; set; }

    [Parameter] public bool BuildingsEnabled { get; set; } = true;

    protected MenuSlot ActiveSlot { get; private set; } = MenuSlot.None;

    protected bool ShowOverview { get; private set; }

    /// <summary>Open dilemmas whose deadline is this month or already missed.</summary>
    protected int AttentionCount => Dilemmas.Count(d =>
        d.Status != DilemmaStatus.Completed &&
        (d.IsMissed(CurrentMonth) || d.MonthsLeft(CurrentMonth) == 0));

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

    /// <summary>Opens the Doelen panel, e.g. after a new dilemma was received.</summary>
    public void OpenDilemmas()
    {
        ActiveSlot = MenuSlot.Dilemmas;
        StateHasChanged();
    }

    protected void OpenOverview() => ShowOverview = true;

    protected void CloseOverview() => ShowOverview = false;
}
