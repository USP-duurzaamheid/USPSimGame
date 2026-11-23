using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Exceptions;
using USPSimGame.Services;
using USPSimGame.Services.Auth;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.Teams;
using USPSimGame.Services.Plans;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace USPSimGame.Components.Creator.Sessions;

public partial class AddGameSessionForm : ComponentBase
{
    [Inject]
    private AuthenticationStateProvider AuthStateProdiver { get; set; }


    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Parameter]
    public EventCallback<GameSession> OnSessionAdded { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    protected GameSession NewSession { get; set; } = new()
    {
        Name = "Utrecht Science Park",
        CenterLatLong = "52.08640, 5.17516",
        OwnerId = ""
    };

    protected string? ErrorMessage { get; set; }
    protected bool IsSubmitting { get; set; }

    protected void SetPreset(string name, string coords)
    {
        NewSession.Name = name;
        NewSession.CenterLatLong = coords;
    }

    protected async Task HandleSubmit()
    {
        var authState = await AuthStateProdiver.GetAuthenticationStateAsync();
        string? adminID = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminID is not null) NewSession.OwnerId = adminID;

        ErrorMessage = null;
        IsSubmitting = true;

        try
        {
            var created = await GameSessionService.CreateGameSessionAsync(NewSession);
            await OnSessionAdded.InvokeAsync(created);
        }
        catch (JoinCodeAllocationException)
        {
            ErrorMessage = "Could not generate a game code. Please try again.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create game session: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
