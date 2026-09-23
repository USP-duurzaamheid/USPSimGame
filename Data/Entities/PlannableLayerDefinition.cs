namespace USPSimGame.Data.Entities;

public class PlannableLayerDefinition
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MapLayerCategory Category { get; set; } = MapLayerCategory.Infrastructure;
    public PlannableGeometryType GeometryType { get; set; } = PlannableGeometryType.Polygon;
    public string? Icon { get; set; }
    public string? DefaultColor { get; set; }
    public double? DefaultLineWidthPx { get; set; }
    public string? TranslatorTags { get; set; }
    public string? SimulatorTags { get; set; }
    public bool IsEnabledByDefault { get; set; } = false;
    public double BaseInvestmentPoints { get; set; } = 0.0;
    public double InvestmentPointsPerUnit { get; set; } = 30;
    public double BaseMonthlyExpensePoints { get; set; } = 0.0;
    public double MonthlyExpensePointsPerUnit { get; set; } = 1;
    public int BaseConstructionTimeMonths { get; set; } = 0;
    public double ConstructionTimeModifierPerUnit { get; set; } = 0.0;
}
