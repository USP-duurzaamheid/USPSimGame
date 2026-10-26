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

    private readonly List<DilemmaViewModel> receivedDilemmas = [];

    /// <summary>Dilemma shown in the new-dilemma pop-up, if any.</summary>
    protected DilemmaViewModel? IncomingDilemma { get; private set; }

    /// <summary>Code of the most recently received dilemma, marked as "Nieuw" in the Doelen list.</summary>
    protected string? NewDilemmaCode { get; private set; }

    /// <summary>The dilemmas from the parent plus the ones received during this game.</summary>
    protected IReadOnlyList<DilemmaViewModel> AllDilemmas =>
        receivedDilemmas
            .Concat(Dilemmas.Where(d => receivedDilemmas.All(r => r.Code != d.Code)))
            .ToList();

    /// <summary>Open dilemmas whose deadline is this month or already missed.</summary>
    protected int AttentionCount => AllDilemmas.Count(d =>
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

    /// <summary>
    /// Adds a new dilemma to Doelen and shows the new-dilemma pop-up.
    /// Call this when the team receives a dilemma during the game.
    /// </summary>
    public void ReceiveDilemma(DilemmaViewModel dilemma)
    {
        receivedDilemmas.RemoveAll(d => d.Code == dilemma.Code);
        receivedDilemmas.Insert(0, dilemma);
        NewDilemmaCode = dilemma.Code;
        IncomingDilemma = dilemma;
        StateHasChanged();
    }

    protected void CloseIncomingDilemma() => IncomingDilemma = null;

    protected void ViewIncomingDilemma()
    {
        IncomingDilemma = null;
        ActiveSlot = MenuSlot.Dilemmas;
    }

    protected void OpenOverview() => ShowOverview = true;

    protected void CloseOverview() => ShowOverview = false;
}
