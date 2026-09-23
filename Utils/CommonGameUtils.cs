using System;
using USPSimGame.Data.Enums;

namespace USPSimGame.Utils;

public static class CommonGameUtils
{
    public static string GetGameStateBadgeClass(this GameState state) => state switch
    {
        GameState.Setup => "bg-info text-dark",
        GameState.Play => "bg-success",
        GameState.Pause => "bg-warning text-dark",
        GameState.Simulation => "bg-primary",
        GameState.Complete => "bg-secondary",
        _ => "bg-secondary"
    };

    public static string FormatMonthYear(int startMonth, int startYear = 2026)
    {
        int totalMonths = (startYear * 12) + startMonth;
        int year = totalMonths / 12;
        int month = (totalMonths % 12) + 1;
        DateTime dt = new DateTime(year, month, 1);
        return $"{dt:MMM yyyy}";
    }
}
