using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;
using USPSimGame.Utils;

namespace USPSimGame.Services.Plans;

public class PlanApprovalEvaluationService : IPlanApprovalEvaluationService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public PlanApprovalEvaluationService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PlanApprovalEvaluation> EvaluatePlanAsync(int planId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var plan = await context.Plans
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            return new PlanApprovalEvaluation
            {
                HasGeometry = false,
                RequiresMultiTeamApproval = false,
                ExplanatoryText = "Plan not found.",
                AllowedStates = new List<PlanState> { PlanState.Draft, PlanState.Archived }
            };
        }

        var featureGeoJsons = plan.Features
            .Where(f => !string.IsNullOrWhiteSpace(f.GeoJsonGeometry))
            .Select(f => f.GeoJsonGeometry!)
            .ToList();

        return await EvaluatePlanGeometryAsync(plan.GameSessionId, plan.TeamId, featureGeoJsons);
    }

    public async Task<PlanApprovalEvaluation> EvaluatePlanGeometryAsync(int gameSessionId, int proposingTeamId, IEnumerable<string> geoJsonFeatures)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var teams = await context.Teams
            .Where(t => t.GameSessionId == gameSessionId)
            .ToListAsync();

        var result = new PlanApprovalEvaluation();
        var planRings = new List<List<GeoJsonSpatialUtils.Point2D>>();

        foreach (var gj in geoJsonFeatures)
        {
            var rings = GeoJsonSpatialUtils.ExtractRings(gj);
            if (rings.Count > 0) planRings.AddRange(rings);
        }

        if (planRings.Count == 0)
        {
            result.HasGeometry = false;
            result.RequiresMultiTeamApproval = false;
            result.ExplanatoryText = "This plan has no drawn features yet. Add geometry before submitting for consultation or approval.";
            result.AllowedStates = new List<PlanState> { PlanState.Draft, PlanState.Archived };
            return result;
        }

        result.HasGeometry = true;
        var requiredTeamIdsSet = new HashSet<int>();

        // 1. Check Condition A: Overlap with Implemented Plans owned by OTHER teams
        var implementedPlans = await context.Plans
            .Include(p => p.Features)
            .Where(p => p.GameSessionId == gameSessionId && p.State == PlanState.Implemented)
            .ToListAsync();

        foreach (var implPlan in implementedPlans)
        {
            if (implPlan.TeamId == proposingTeamId) continue;

            var implRings = new List<List<GeoJsonSpatialUtils.Point2D>>();
            foreach (var feat in implPlan.Features)
            {
                var r = GeoJsonSpatialUtils.ExtractRings(feat.GeoJsonGeometry);
                if (r.Count > 0) implRings.AddRange(r);
            }

            if (implRings.Count > 0 && GeoJsonSpatialUtils.DoRingsIntersect(planRings, implRings))
            {
                result.ConditionA_OverlapsOtherImplementedPlan = true;
                requiredTeamIdsSet.Add(implPlan.TeamId);
            }
        }

        // 2. Check Condition B: Overlap with Team Territory Polygons
        bool overlapsOwnTerritory = false;
        foreach (var team in teams)
        {
            var territoryRings = GeoJsonSpatialUtils.ExtractRings(team.AreaDefinition);
            if (territoryRings.Count > 0 && GeoJsonSpatialUtils.DoRingsIntersect(planRings, territoryRings))
            {
                if (team.Id == proposingTeamId)
                {
                    overlapsOwnTerritory = true;
                }
                else
                {
                    result.ConditionB_OverlapsOtherTeamTerritory = true;
                    requiredTeamIdsSet.Add(team.Id);
                }
            }
        }

        // 3. Condition C/D: New Unclaimed Development (Check if ANY feature/ring lies outside all team territories)
        var allTeamTerritoryRings = teams
            .SelectMany(t => GeoJsonSpatialUtils.ExtractRings(t.AreaDefinition))
            .ToList();

        bool hasUnclaimedTerritoryFeature = false;
        foreach (var ring in planRings)
        {
            var singleRingList = new List<List<GeoJsonSpatialUtils.Point2D>> { ring };
            bool intersectsAnyTeamTerritory = allTeamTerritoryRings.Count > 0 &&
                GeoJsonSpatialUtils.DoRingsIntersect(singleRingList, allTeamTerritoryRings);

            if (!intersectsAnyTeamTerritory)
            {
                hasUnclaimedTerritoryFeature = true;
                break;
            }
        }

        if (hasUnclaimedTerritoryFeature || (!overlapsOwnTerritory && !result.ConditionA_OverlapsOtherImplementedPlan && !result.ConditionB_OverlapsOtherTeamTerritory))
        {
            result.IsNewUnclaimedDevelopment = true;
            foreach (var team in teams)
            {
                if (team.Id != proposingTeamId) requiredTeamIdsSet.Add(team.Id);
            }
        }

        result.RequiredTeamIds = requiredTeamIdsSet.ToList();
        result.RequiredTeamNames = teams
            .Where(t => result.RequiredTeamIds.Contains(t.Id))
            .Select(t => t.Name)
            .ToList();

        result.RequiresMultiTeamApproval = result.RequiredTeamIds.Count > 0;

        if (result.RequiresMultiTeamApproval)
        {
            var teamListText = string.Join(", ", result.RequiredTeamNames);
            if (result.IsNewUnclaimedDevelopment)
            {
                result.ExplanatoryText = "This plan adds something completely new to the map. Approval from all teams is required.";
            }
            else if (result.ConditionA_OverlapsOtherImplementedPlan && result.ConditionB_OverlapsOtherTeamTerritory)
            {
                result.ExplanatoryText = $"This plan overlaps implemented geometry and territories owned by other teams ({teamListText}). Approval is required from {teamListText} before this plan can be approved.";
            }
            else if (result.ConditionB_OverlapsOtherTeamTerritory)
            {
                result.ExplanatoryText = $"This plan overlaps territory belonging to {teamListText}. Approval is required from {teamListText} before this plan can be approved.";
            }
            else if (result.ConditionA_OverlapsOtherImplementedPlan)
            {
                result.ExplanatoryText = $"This plan overlaps existing implemented features owned by {teamListText}. Approval is required from {teamListText} before this plan can be approved.";
            }
            else
            {
                result.ExplanatoryText = "This plan adds something completely new to the map. Approval from all teams is required.";
            }

            result.AllowedStates = new List<PlanState>
            {
                PlanState.Draft,
                PlanState.Consultation,
                PlanState.Requested,
                PlanState.Archived
            };
        }
        else
        {
            result.ExplanatoryText = "This plan lies within your team's territory with no conflicting external overlays. You may approve it directly.";
            result.AllowedStates = new List<PlanState>
            {
                PlanState.Draft,
                PlanState.Consultation,
                PlanState.Requested,
                PlanState.Approved,
                PlanState.Archived
            };
        }

        return result;
    }
}
