using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Models;
using USPSimGame.Utils;

namespace USPSimGame.Components.Game.ControlBarFiles;

public partial class StatFines : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<FineModel> Fines { get; set; } = [];
    [Parameter] public int StartYear { get; set; } = 2026;
    [Parameter] public bool IsPreview { get; set; }

    protected int ActiveCount => Fines.Count(f => f.IsActive);

    protected IEnumerable<FineModel> OrderedFines => Fines
        .OrderByDescending(f => f.IsActive)
        .ThenByDescending(f => f.StartedMonth);

    protected string Subtitle
    {
        get
        {
            var total = Fines.Where(f => f.IsActive).Sum(f => f.AmountCredits);
            return total > 0 ? $"Je betaalt nu -{total} CC per maand aan boetes" : "Er lopen nu geen boetes";
        }
    }

    protected string GetStatusLabel(FineModel fine) => fine.EndedMonth is int endedMonth
        ? $"Gestopt in {CommonGameUtils.FormatMonthYear(endedMonth, StartYear)}"
        : $"Loopt sinds {CommonGameUtils.FormatMonthYear(fine.StartedMonth, StartYear)}";
}
