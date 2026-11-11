using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using USPSimGame.Data.Models;
using USPSimGame.Utils;

namespace USPSimGame.Components.Game.Dilemmas;

public partial class DilemmaPopup : ComponentBase
{
    [Parameter, EditorRequired] public DilemmaModel Dilemma { get; set; } = default!;
    [Parameter] public int CurrentMonth { get; set; }
    [Parameter] public int StartYear { get; set; } = 2026;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnViewGoals { get; set; }

    private ElementReference primaryButton;

    protected string DeadlineLabel => CommonGameUtils.FormatMonthYear(Dilemma.DeadlineMonth, StartYear);

    protected string TimeLeftLabel
    {
        get
        {
            var monthsLeft = Dilemma.MonthsLeft(CurrentMonth);
            return monthsLeft switch
            {
                <= 0 => "alleen deze maand nog",
                1 => "nog 1 maand",
                < 24 => $"nog {monthsLeft} maanden",
                _ => $"nog {monthsLeft / 12} jaar"
            };
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await primaryButton.FocusAsync();
        }
    }

    protected Task CloseAsync() => OnClose.InvokeAsync();

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await CloseAsync();
        }
    }
}
