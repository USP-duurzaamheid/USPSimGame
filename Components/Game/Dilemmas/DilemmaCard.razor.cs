using Microsoft.AspNetCore.Components;
using USPSimGame.Utils;

namespace USPSimGame.Components.Game.Dilemmas;

/// <summary>
/// One dilemma: its assignment, how many months are left, the reward and the fine.
/// A missed deadline is shown in red, a deadline in the current month in amber.
/// </summary>
public partial class DilemmaCard : ComponentBase
{
    [Parameter, EditorRequired] public DilemmaViewModel Dilemma { get; set; } = default!;

    /// <summary>Current game month of the session (0 = first month).</summary>
    [Parameter] public int CurrentMonth { get; set; }

    [Parameter] public int StartYear { get; set; } = 2026;

    protected bool IsMissed => Dilemma.IsMissed(CurrentMonth);

    protected bool IsUrgent => !IsMissed && Dilemma.MonthsLeft(CurrentMonth) == 0;

    protected string DeadlineLabel => CommonGameUtils.FormatMonthYear(Dilemma.DeadlineMonth, StartYear);

    protected string TimeLeftLabel
    {
        get
        {
            if (IsMissed) return $"Deadline gemist ({DeadlineLabel})";

            var monthsLeft = Dilemma.MonthsLeft(CurrentMonth);
            return monthsLeft switch
            {
                0 => "Laatste maand",
                1 => "Nog 1 maand",
                < 24 => $"Nog {monthsLeft} maanden",
                _ => $"Nog {monthsLeft / 12} jaar"
            };
        }
    }

    protected string CategoryLabel => Dilemma.Category switch
    {
        DilemmaCategory.Guided => "Introductie",
        DilemmaCategory.LongTerm => "Lange termijn",
        _ => "Opdracht"
    };
}
