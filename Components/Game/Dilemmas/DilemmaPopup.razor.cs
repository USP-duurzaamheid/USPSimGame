using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using USPSimGame.Utils;

namespace USPSimGame.Components.Game.Dilemmas;

/// <summary>
/// Pop-up shown when the team receives a new dilemma: the assignment, the deadline,
/// the reward and the fine. The dilemma is already added to the Doelen list.
/// </summary>
public partial class DilemmaPopup : ComponentBase
{
    [Parameter, EditorRequired] public DilemmaViewModel Dilemma { get; set; } = default!;

    /// <summary>Current game month of the session (0 = first month).</summary>
    [Parameter] public int CurrentMonth { get; set; }

    [Parameter] public int StartYear { get; set; } = 2026;

    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>Closes the pop-up and opens the Doelen overview.</summary>
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
