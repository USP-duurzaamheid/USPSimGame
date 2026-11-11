using USPSimGame.Data.Models;

namespace USPSimGame.Data.Preview;

public static partial class GamePreviewData
{
    public static readonly NoisePollutionInfo PreviewNoisePollutionInfo = new()
    {
        Limit = 30,
        Sources = new()
        {
            new() { Name = "Zonnepaneel", Icon = "bi-sun", Count = 1, PointsPerUnit = 1 },
            new() { Name = "Dieselgenerator", Icon = "bi-fuel-pump", Count = 1, PointsPerUnit = 12 },
            new() { Name = "Windturbine", Icon = "bi-wind", Count = 0, PointsPerUnit = 18 }
        }
    };
}
