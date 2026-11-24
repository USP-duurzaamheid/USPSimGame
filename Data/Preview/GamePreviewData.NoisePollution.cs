using USPSimGame.Data.Models;

namespace USPSimGame.Data.Preview;

public static partial class GamePreviewData
{
    public static readonly NoisePollutionInfo PreviewNoisePollutionInfo = new()
    {
        Limit = 30,
        Sources = new()
        {
            new() { Name = "Solar panel", Icon = "bi-sun", Count = 1, PointsPerUnit = 1 },
            new() { Name = "Diesel generator", Icon = "bi-fuel-pump", Count = 1, PointsPerUnit = 12 },
            new() { Name = "Wind turbine", Icon = "bi-wind", Count = 0, PointsPerUnit = 18 }
        }
    };
}
