using USPSimGame.Data.Entities;

namespace USPSimGame.Services.Plans;

public class PlanApprovalEvaluation
{
    public bool HasGeometry { get; set; }
    public bool RequiresMultiTeamApproval { get; set; }
    public List<int> RequiredTeamIds { get; set; } = new();
    public List<string> RequiredTeamNames { get; set; } = new();
    public string ExplanatoryText { get; set; } = string.Empty;
    public List<PlanState> AllowedStates { get; set; } = new();
    public bool ConditionA_OverlapsOtherImplementedPlan { get; set; }
    public bool ConditionB_OverlapsOtherTeamTerritory { get; set; }
    public bool IsNewUnclaimedDevelopment { get; set; }
}

public interface IPlanApprovalEvaluationService
{
    Task<PlanApprovalEvaluation> EvaluatePlanAsync(int planId);
    Task<PlanApprovalEvaluation> EvaluatePlanGeometryAsync(int gameSessionId, int proposingTeamId, IEnumerable<string> geoJsonFeatures);
}
