using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Costing;

public struct PlanCostEstimate
{
    public double TotalInvestmentPoints { get; set; }
    public double TotalMonthlyExpensePoints { get; set; }
    public double ConfirmedPerTeamInvestmentShare { get; set; }
    public double ConfirmedPerTeamMonthlyExpenseShare { get; set; }
    public double PotentialPerTeamInvestmentShare { get; set; }
    public double PotentialPerTeamMonthlyExpenseShare { get; set; }
    public int ConfirmedJoinedTeamCount { get; set; }
    public int PotentialTotalTeamCount { get; set; }
}

public interface ICostCalculationService
{
    PlanCostEstimate CalculateFeatureCost(PlannableLayerDefinition definition, string? geoJson, bool isDemolition = false);
    PlanCostEstimate CalculateDraftPlanCost(IEnumerable<(PlannableLayerDefinition def, string? geoJson, bool isDemolition)> features, int confirmedJoinedTeams = 1, int potentialTotalTeams = 1);
    PlanCostEstimate CalculateDraftPlanCost(IEnumerable<(PlannableLayerDefinition def, string? geoJson)> features, int confirmedJoinedTeams = 1, int potentialTotalTeams = 1);
    Task<PlanCostEstimate> CalculatePlanCostAsync(int planId);

    int CalculateFeatureConstructionTimeMonths(PlannableLayerDefinition definition, string? geoJson);
    int CalculateDraftPlanConstructionTimeMonths(IEnumerable<(PlannableLayerDefinition def, string? geoJson)> features);
    Task<int> CalculatePlanConstructionTimeMonthsAsync(int planId);
}
