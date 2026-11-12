using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Game.Teams;

public partial class TeamBadge : ComponentBase
{
    private const string FallbackColor = "#5cb88d";

    [Parameter, EditorRequired] public string Name { get; set; } = string.Empty;
    [Parameter] public string? Color { get; set; }

    protected string BackgroundColor => IsHexColor(Color) ? Color! : FallbackColor;

    protected string Initials
    {
        get
        {
            var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !w.Equals("Team", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (words.Length == 0) return "?";

            if (words.Length == 1)
            {
                var word = words[0];
                return (word.Length <= 4 ? word : word[..2]).ToUpperInvariant();
            }

            return string.Concat(words.Take(3).Select(w => char.ToUpperInvariant(w[0])));
        }
    }

    protected string TextColor
    {
        get
        {
            var hex = BackgroundColor.TrimStart('#');
            var r = int.Parse(hex[..2], NumberStyles.HexNumber);
            var g = int.Parse(hex[2..4], NumberStyles.HexNumber);
            var b = int.Parse(hex[4..6], NumberStyles.HexNumber);
            var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
            return luminance > 0.6 ? "#182c25" : "#ffffff";
        }
    }

    private static bool IsHexColor(string? value) =>
        value is { Length: 7 } && value[0] == '#' &&
        int.TryParse(value[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);
}
