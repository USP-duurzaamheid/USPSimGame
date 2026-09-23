using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;
using USPSimGame.Services.Auth;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.Teams;
using USPSimGame.Services.Plans;

namespace USPSimGame.Components.Creator.Sessions;

public partial class AddGameSessionForm : ComponentBase
{
    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Parameter]
    public EventCallback<GameSession> OnSessionAdded { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    protected GameSession NewSession { get; set; } = new()
    {
        Name = "Utrecht Science Park",
        CenterLatLong = "52.08640, 5.17516"
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
        ErrorMessage = null;
        IsSubmitting = true;

        try
        {
            var created = await GameSessionService.CreateGameSessionAsync(NewSession);
            await OnSessionAdded.InvokeAsync(created);
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
