using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Services.Teams;

namespace USPSimGame.Components.Pages;

public partial class JoinTeam : ComponentBase
{
    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Inject]
    public ITeamService TeamService { get; set; } = default!;

    [Inject]
    public IPlayerSessionService PlayerSessionService { get; set; } = default!;

    [Inject]
    public PlayerSessionState PlayerSessionState { get; set; } = default!;

    [Inject]
    public ClientConnectionInfo ClientConnection { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    [Inject]
    public ILogger<JoinTeam> Logger { get; set; } = default!;

    [Parameter]
    public string? Code { get; set; }

    protected GameSession? Session { get; set; }
    protected List<Team> Teams { get; set; } = [];

    protected int? SelectedTeamId { get; set; }
    protected string PlayerName { get; set; } = string.Empty;

    protected string? ErrorMessage { get; set; }
    protected bool IsJoining { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // The code comes from the URL, so it is never trusted: it is validated and resolved again here.
            var result = await GameSessionService.FindJoinableByJoinCodeAsync(Code ?? string.Empty, ClientConnection.ClientKey);
            if (!result.Success || result.GameSession == null)
            {
                Navigation.NavigateTo("/");
                return;
            }

            Session = result.GameSession;
            Teams = await TeamService.GetTeamsByGameSessionAsync(Session.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "JoinTeam: Error while loading the game session and its teams");
            ErrorMessage = "Something went wrong. Please try again.";
        }
    }

    // Placeholder until the design decides what to show in the badge: first letters of up to 3 words of the team name.
    protected static string GetInitials(string teamName) =>
        string.Concat(teamName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3).Select(w => char.ToUpperInvariant(w[0])));

    protected async Task EnterSessionAsync()
    {
        if (SelectedTeamId == null || IsJoining)
        {
            return;
        }

        IsJoining = true;
        ErrorMessage = null;

        try
        {
            var result = await PlayerSessionService.JoinByCodeAsync(Code ?? string.Empty, SelectedTeamId.Value, PlayerName, ClientConnection.ClientKey);
            if (result.Success && result.PlayerSession != null && result.Team != null && result.GameSession != null)
            {
                PlayerSessionState.SetSession(result.PlayerSession, result.Team, result.GameSession);
                Navigation.NavigateTo("/game");
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "JoinTeam: Error while joining team {TeamId}", SelectedTeamId);
            ErrorMessage = "Something went wrong. Please try again.";
        }
        finally
        {
            IsJoining = false;
        }
    }
}
