using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace USPSimGame.Components.Game.Dilemmas;

/// <summary>
/// Overview of the team's active dilemmas ("Doelen"). Regular dilemmas are listed first,
/// with missed deadlines on top; long-term goals have their own section.
/// Completed dilemmas are not shown.
/// </summary>
public partial class ActiveDilemmasPanel : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<DilemmaViewModel> Dilemmas { get; set; } = [];

    /// <summary>Current game month of the session (0 = first month).</summary>
    [Parameter] public int CurrentMonth { get; set; }

    [Parameter] public int StartYear { get; set; } = 2026;

    /// <summary>Marks the list as example data that is not yet provided by the backend.</summary>
    [Parameter] public bool IsPreview { get; set; }

    [Parameter] public EventCallback OnClose { get; set; }

    private ElementReference panel;

    protected List<DilemmaViewModel> RegularDilemmas => OpenDilemmas
        .Where(d => d.Category != DilemmaCategory.LongTerm)
        .ToList();

    protected List<DilemmaViewModel> LongTermDilemmas => OpenDilemmas
        .Where(d => d.Category == DilemmaCategory.LongTerm)
        .ToList();

    private IEnumerable<DilemmaViewModel> OpenDilemmas => Dilemmas
        .Where(d => d.Status != DilemmaStatus.Completed)
        .OrderByDescending(d => d.IsMissed(CurrentMonth))
        .ThenBy(d => d.DeadlineMonth);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await panel.FocusAsync();
        }
    }

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await OnClose.InvokeAsync();
        }
    }
}
