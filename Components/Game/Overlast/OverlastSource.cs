namespace USPSimGame.Components.Game.Overlast;

/// <summary>
/// One type of installation that causes overlast (noise), as shown in the overlast meter.
/// </summary>
/// <param name="Name">Display name, e.g. "Windturbine".</param>
/// <param name="Icon">Bootstrap icon class, e.g. "bi-wind".</param>
/// <param name="Count">How many of these the team has placed.</param>
/// <param name="PointsPerUnit">Overlast points caused by one installation.</param>
public record OverlastSource(string Name, string Icon, int Count, int PointsPerUnit)
{
    public int TotalPoints => Count * PointsPerUnit;
}
