using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Game.Plans;

public static class PlanStateLabels
{
    public static string Get(PlanState state) => state switch
    {
        PlanState.Implementing => "Under construction",
        PlanState.Implemented => "Active",
        _ => state.ToString()
    };
}
