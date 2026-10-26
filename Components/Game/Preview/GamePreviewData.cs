using USPSimGame.Components.Game.Overlast;

namespace USPSimGame.Components.Game.Preview;

/// <summary>
/// Example values for parts of the game UI that the backend does not provide yet.
/// Every value shown from here is marked as "voorbeeldwaarde" in its tooltip.
/// Replace a value with real data as soon as the matching backend story is done,
/// and remove it from this class.
/// </summary>
public static class GamePreviewData
{
    /// <summary>Sustainability points earned by completing challenges.</summary>
    public const int SustainabilityPoints = 100;

    /// <summary>Share of renewable electricity production, in percent.</summary>
    public const int RenewableElectricityPercent = 48;

    /// <summary>Number of Campus Credit fines received for missed deadlines.</summary>
    public const int FineCount = 1;

    /// <summary>
    /// Installations causing overlast for the team. Points per unit follow the
    /// "Game Journey" MVP balancing values (solar +1, wind +18, diesel +12).
    /// </summary>
    public static readonly IReadOnlyList<OverlastSource> OverlastSources =
    [
        new("Zonnepaneel", "bi-sun", 1, 1),
        new("Dieselgenerator", "bi-fuel-pump", 1, 12),
        new("Windturbine", "bi-wind", 0, 18),
    ];

    /// <summary>Overlast limit used by challenges such as M-02 ("overlast at or below 30").</summary>
    public const int OverlastLimit = 30;
}
