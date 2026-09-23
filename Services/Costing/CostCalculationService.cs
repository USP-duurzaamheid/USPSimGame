using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;
using USPSimGame.Utils;

namespace USPSimGame.Services.Costing;

public class CostCalculationService : ICostCalculationService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public CostCalculationService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public PlanCostEstimate CalculateFeatureCost(PlannableLayerDefinition definition, string? geoJson, bool isDemolition = false)
    {
        if (definition == null || string.IsNullOrWhiteSpace(geoJson))
        {
            return new PlanCostEstimate();
        }

        double quantity = GeoJsonSpatialUtils.CalculateFeatureQuantity(geoJson, definition.GeometryType);
        double investment;
        double monthlyExpense;

        if (isDemolition)
        {
            investment = Math.Round(definition.BaseInvestmentPoints + (quantity * definition.InvestmentPointsPerUnit), 1);
            monthlyExpense = -Math.Round(definition.BaseMonthlyExpensePoints + (quantity * definition.MonthlyExpensePointsPerUnit), 1);
        }
        else
        {
            investment = Math.Round(definition.BaseInvestmentPoints + (quantity * definition.InvestmentPointsPerUnit), 1);
            monthlyExpense = Math.Round(definition.BaseMonthlyExpensePoints + (quantity * definition.MonthlyExpensePointsPerUnit), 1);
        }

        return new PlanCostEstimate
        {
            TotalInvestmentPoints = investment,
            TotalMonthlyExpensePoints = monthlyExpense,
            ConfirmedPerTeamInvestmentShare = investment,
            ConfirmedPerTeamMonthlyExpenseShare = monthlyExpense,
            PotentialPerTeamInvestmentShare = investment,
            PotentialPerTeamMonthlyExpenseShare = monthlyExpense,
            ConfirmedJoinedTeamCount = 1,
            PotentialTotalTeamCount = 1
        };
    }

    public PlanCostEstimate CalculateDraftPlanCost(IEnumerable<(PlannableLayerDefinition def, string? geoJson, bool isDemolition)> features, int confirmedJoinedTeams = 1, int potentialTotalTeams = 1)
    {
        double totalInvestment = 0;
        double totalMonthlyExpense = 0;

        foreach (var (def, geoJson, isDemolition) in features)
        {
            if (def != null && !string.IsNullOrWhiteSpace(geoJson))
            {
                var est = CalculateFeatureCost(def, geoJson, isDemolition);
                totalInvestment += est.TotalInvestmentPoints;
                totalMonthlyExpense += est.TotalMonthlyExpensePoints;
            }
        }

        confirmedJoinedTeams = Math.Max(1, confirmedJoinedTeams);
        potentialTotalTeams = Math.Max(confirmedJoinedTeams, potentialTotalTeams);

        return new PlanCostEstimate
        {
            TotalInvestmentPoints = Math.Round(totalInvestment, 1),
            TotalMonthlyExpensePoints = Math.Round(totalMonthlyExpense, 1),
            ConfirmedPerTeamInvestmentShare = Math.Round(totalInvestment / confirmedJoinedTeams, 1),
            ConfirmedPerTeamMonthlyExpenseShare = Math.Round(totalMonthlyExpense / confirmedJoinedTeams, 1),
            PotentialPerTeamInvestmentShare = Math.Round(totalInvestment / potentialTotalTeams, 1),
            PotentialPerTeamMonthlyExpenseShare = Math.Round(totalMonthlyExpense / potentialTotalTeams, 1),
            ConfirmedJoinedTeamCount = confirmedJoinedTeams,
            PotentialTotalTeamCount = potentialTotalTeams
        };
    }

    public PlanCostEstimate CalculateDraftPlanCost(IEnumerable<(PlannableLayerDefinition def, string? geoJson)> features, int confirmedJoinedTeams = 1, int potentialTotalTeams = 1)
    {
        return CalculateDraftPlanCost(features.Select(f => (f.def, f.geoJson, false)), confirmedJoinedTeams, potentialTotalTeams);
    }

    public async Task<PlanCostEstimate> CalculatePlanCostAsync(int planId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var plan = await context.Plans
            .Include(p => p.Features)
                .ThenInclude(f => f.GameSessionPlannableLayer)
                    .ThenInclude(l => l!.PlannableLayerDefinition)
            .Include(p => p.Judgments)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null) return new PlanCostEstimate();

        var featureTuples = plan.Features
            .Where(f => f.GameSessionPlannableLayer?.PlannableLayerDefinition != null && !string.IsNullOrWhiteSpace(f.GeoJsonGeometry))
            .Select(f => (f.GameSessionPlannableLayer!.PlannableLayerDefinition, f.GeoJsonGeometry, f.IsDemolition));

        int joinedCount = 1 + plan.Judgments.Count(j => j.Judgment == PlanJudgmentType.Join);
        int totalSessionTeams = await context.Teams.CountAsync(t => t.GameSessionId == plan.GameSessionId);
        int potentialTeams = Math.Max(joinedCount, totalSessionTeams);

        return CalculateDraftPlanCost(featureTuples, joinedCount, potentialTeams);
    }

    public int CalculateFeatureConstructionTimeMonths(PlannableLayerDefinition definition, string? geoJson)
    {
        if (definition == null || string.IsNullOrWhiteSpace(geoJson))
        {
            return 0;
        }

        double quantity = GeoJsonSpatialUtils.CalculateFeatureQuantity(geoJson, definition.GeometryType);
        double rawMonths = definition.BaseConstructionTimeMonths + (quantity * definition.ConstructionTimeModifierPerUnit);
        return (int)Math.Ceiling(Math.Max(0, rawMonths));
    }

    public int CalculateDraftPlanConstructionTimeMonths(IEnumerable<(PlannableLayerDefinition def, string? geoJson)> features)
    {
        int maxMonths = 0;
        foreach (var (def, geoJson) in features)
        {
            if (def != null && !string.IsNullOrWhiteSpace(geoJson))
            {
                int layerMonths = CalculateFeatureConstructionTimeMonths(def, geoJson);
                maxMonths = Math.Max(maxMonths, layerMonths);
            }
        }
        return maxMonths;
    }

    public async Task<int> CalculatePlanConstructionTimeMonthsAsync(int planId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var plan = await context.Plans
            .Include(p => p.Features)
                .ThenInclude(f => f.GameSessionPlannableLayer)
                    .ThenInclude(l => l!.PlannableLayerDefinition)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null) return 0;

        var featurePairs = plan.Features
            .Where(f => f.GameSessionPlannableLayer?.PlannableLayerDefinition != null && !string.IsNullOrWhiteSpace(f.GeoJsonGeometry))
            .Select(f => (f.GameSessionPlannableLayer!.PlannableLayerDefinition, f.GeoJsonGeometry));

        return CalculateDraftPlanConstructionTimeMonths(featurePairs);
    }
}
