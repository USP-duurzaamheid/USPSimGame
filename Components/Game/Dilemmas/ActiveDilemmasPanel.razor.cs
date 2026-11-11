using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;

namespace USPSimGame.Components.Game.Dilemmas;

public partial class ActiveDilemmasPanel : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<DilemmaModel> Dilemmas { get; set; } = [];
    [Parameter] public int CurrentMonth { get; set; }
    [Parameter] public int StartYear { get; set; } = 2026;
    [Parameter] public string? NewDilemmaCode { get; set; }
    [Parameter] public bool IsPreview { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private ElementReference panel;

    protected List<DilemmaModel> RegularDilemmas => OpenDilemmas
        .Where(d => d.Category != DilemmaCategory.LongTerm)
        .ToList();

    protected List<DilemmaModel> LongTermDilemmas => OpenDilemmas
        .Where(d => d.Category == DilemmaCategory.LongTerm)
        .ToList();

    private IEnumerable<DilemmaModel> OpenDilemmas => Dilemmas
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
