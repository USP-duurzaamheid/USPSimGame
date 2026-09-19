using USPSimGame.Application.Services;

namespace USPSimGame.Web.Endpoints;

public static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/teams/session/{sessionId:int}",
            async (int sessionId, ITeamService teamService) =>
            {
                var teams = await teamService.GetTeamsByGameSessionAsync(sessionId);
                var payload = teams.Select(team => new
                {
                    id = team.Id,
                    name = team.Name,
                    color = team.Color,
                    areaDefinition = team.AreaDefinition
                });
                return Results.Ok(payload);
            });

        return endpoints;
    }
}
