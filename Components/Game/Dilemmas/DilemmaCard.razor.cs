using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Enums;
using USPSimGame.Data.Models;
using USPSimGame.Utils;

namespace USPSimGame.Components.Game.Dilemmas;

public partial class DilemmaCard : ComponentBase
{
    [Parameter, EditorRequired] public DilemmaModel Dilemma { get; set; } = default!;
    [Parameter] public int CurrentMonth { get; set; }
    [Parameter] public int StartYear { get; set; } = 2026;
    [Parameter] public bool IsNew { get; set; }

    protected bool IsMissed => Dilemma.IsMissed(CurrentMonth);

    protected bool IsUrgent => Dilemma.IsDueThisMonth(CurrentMonth);

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
