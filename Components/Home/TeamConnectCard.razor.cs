using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;

namespace USPSimGame.Components.Home;

public partial class TeamConnectCard : ComponentBase
{
    [Inject]
    public ITeamService TeamService { get; set; } = default!;

    [Inject]
    public IPlayerSessionService PlayerSessionService { get; set; } = default!;

    [Inject]
    public IAuthService AuthService { get; set; } = default!;

    [Inject]
    public PlayerSessionState PlayerSessionState { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired]
    public GameSession SelectedSession { get; set; } = default!;

    protected List<Team>? Teams { get; set; }
    protected int SelectedTeamId { get; set; }
    protected string PlayerName { get; set; } = string.Empty;
    protected string TeamPassword { get; set; } = string.Empty;

    protected string AdminUsername { get; set; } = string.Empty;
    protected string AdminPassword { get; set; } = string.Empty;

    protected string? ErrorMessage { get; set; }
    protected bool IsLoadingTeams { get; set; } = true;
    protected bool IsConnecting { get; set; }

    protected Team? SelectedTeam => Teams?.FirstOrDefault(t => t.Id == SelectedTeamId);

    protected override async Task OnParametersSetAsync()
    {
        if (SelectedSession != null)
        {
            IsLoadingTeams = true;
            ErrorMessage = null;
            try
            {
                Teams = await TeamService.GetTeamsByGameSessionAsync(SelectedSession.Id);
                if (Teams != null && Teams.Any())
                {
                    SelectedTeamId = Teams.First().Id;
                }
                else
                {
                    SelectedTeamId = -1; // Default to Admin if no teams exist
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load teams: {ex.Message}";
            }
            finally
            {
                IsLoadingTeams = false;
            }
        }
    }

    protected async Task HandleConnect()
    {
        ErrorMessage = null;
        IsConnecting = true;

        try
        {
            if (SelectedTeamId == -1)
            {
                if (string.IsNullOrWhiteSpace(AdminUsername) || string.IsNullOrWhiteSpace(AdminPassword))
                {
                    ErrorMessage = "Please enter admin username and password.";
                    return;
                }

                var adminUser = await AuthService.AuthenticateAsync(AdminUsername, AdminPassword);
                if (adminUser != null)
                {
                    PlayerSessionState.SetAdminSession(adminUser, SelectedSession);
                    Navigation.NavigateTo("/game");
                }
                else
                {
                    ErrorMessage = "Invalid admin username or password. Please try again.";
                }
            }
            else
            {
                if (SelectedTeamId <= 0)
                {
                    ErrorMessage = "Please select a valid team.";
                    return;
                }

                var result = await PlayerSessionService.ConnectAsync(SelectedTeamId, PlayerName, TeamPassword);
                if (result.Success && result.PlayerSession != null && result.Team != null && result.GameSession != null)
                {
                    PlayerSessionState.SetSession(result.PlayerSession, result.Team, result.GameSession);
                    Navigation.NavigateTo("/game");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Connection failed. Please check your team password.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during connection: {ex.Message}";
        }
        finally
        {
            IsConnecting = false;
        }
    }
}
