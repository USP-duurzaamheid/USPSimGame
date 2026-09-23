using System.Globalization;

namespace USPSimGame.Utils;

/// <summary>
/// High-precision coordinate transformation utility between WGS84 (EPSG:4326) and Dutch National Grid RD New (EPSG:28992).
/// Uses official Schreiber polynomial approximation formulas from Rijkswaterstaat / Kadaster.
/// </summary>
public static class GeoCoordinateConverter
{
    private const double ReferenceLat = 52.15517440;
    private const double ReferenceLon = 5.38720621;
    private const double ReferenceX = 155000.0;
    private const double ReferenceY = 463000.0;

    /// <summary>
    /// Converts WGS84 (Latitude, Longitude) coordinates to Dutch RD New (X, Y) coordinates.
    /// </summary>
    public static (double x, double y) Wgs84ToRd(double lat, double lon)
    {
        double dLat = 0.36 * (lat - ReferenceLat);
        double dLon = 0.36 * (lon - ReferenceLon);

        double x = ReferenceX
            + (190094.945 * dLon)
            - (11841.05 * dLat)
            - (114.221 * dLon * dLon)
            - (32.391 * dLat * dLat)
            - (0.705 * dLon * dLon * dLon)
            - (2.340 * dLon * dLat * dLat);

        double y = ReferenceY
            + (309056.544 * dLat)
            + (3638.893 * dLon * dLon)
            + (73.077 * dLat * dLat)
            - (157.984 * dLon * dLon * dLat)
            + (59.788 * dLat * dLat * dLat);

        return (x, y);
    }

    /// <summary>
    /// Converts Dutch RD New (X, Y) coordinates to WGS84 (Latitude, Longitude) coordinates.
    /// </summary>
    public static (double lat, double lon) RdToWgs84(double x, double y)
    {
        double dX = (x - ReferenceX) * 1e-5;
        double dY = (y - ReferenceY) * 1e-5;

        double dLatSec = (3235.65389 * dY)
                       - (32.58297 * dX * dX)
                       - (0.24750 * dY * dY)
                       - (0.84978 * dX * dX * dY)
                       - (0.06546 * dY * dY * dY)
                       - (0.01709 * dX * dX * dY * dY)
                       - (0.00738 * dX);

        double dLonSec = (5261.30285 * dX)
                       + (105.97818 * dX * dY)
                       + (2.45656 * dX * dY * dY)
                       - (0.81885 * dX * dX * dX)
                       + (0.05594 * dX * dY * dY * dY)
                       - (0.05607 * dX * dX * dX * dY)
                       + (0.01199 * dY);

        double lat = ReferenceLat + (dLatSec / 3600.0);
        double lon = ReferenceLon + (dLonSec / 3600.0);

        return (lat, lon);
    }

    /// <summary>
    /// Parses a comma-separated center coordinate string (e.g., "52.08640, 5.17516" or "5.17516, 52.08640") into latitude and longitude.
    /// </summary>
    public static bool TryParseLatLong(string? centerLatLong, out double lat, out double lon)
    {
        lat = 0;
        lon = 0;

        if (string.IsNullOrWhiteSpace(centerLatLong)) return false;

        var parts = centerLatLong.Split(',');
        if (parts.Length != 2) return false;

        if (double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double v1) &&
            double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double v2))
        {
            lat = (v1 > v2) ? v1 : v2;
            lon = (v1 > v2) ? v2 : v1;
            return true;
        }

        return false;
    }
}
