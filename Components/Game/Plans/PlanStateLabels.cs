using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Game.Plans;

public static class PlanStateLabels
{
    public static string Get(PlanState state) => state switch
    {
        PlanState.Implementing => "In aanbouw",
        PlanState.Implemented => "Actief",
        _ => state.ToString()
    };
}
