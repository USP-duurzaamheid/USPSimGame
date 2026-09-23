using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;

namespace USPSimGame.Components.Creator;

public partial class GameSessionsOverview : ComponentBase, IDisposable
{
    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Inject]
    public IGameSessionNotifierService Notifier { get; set; } = default!;

    [Parameter, EditorRequired]
    public User CurrentUser { get; set; } = default!;

    [Parameter]
    public EventCallback OnLogout { get; set; }

    protected List<GameSession>? Sessions { get; set; }
    protected bool IsLoading { get; set; } = true;
    protected bool ShowAddForm { get; set; }
    protected string? ErrorMessage { get; set; }

    // Subcomponent Modal State
    protected bool ShowLayersModal { get; set; } = false;
    protected bool ShowTeamsModal { get; set; } = false;
    protected GameSession? SelectedSession { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Notifier.OnGameSessionStateChanged += HandleSessionStateChangedAsync;
        await LoadSessionsAsync();
    }

    private async Task HandleSessionStateChangedAsync(GameSession updatedSession)
    {
        await InvokeAsync(LoadSessionsAsync);
    }

    protected async Task LoadSessionsAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Sessions = await GameSessionService.GetGameSessionsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading game sessions: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ToggleAddForm()
    {
        ShowAddForm = !ShowAddForm;
    }

    protected async Task HandleSessionAdded(GameSession newSession)
    {
        ShowAddForm = false;
        await LoadSessionsAsync();
    }

    protected async Task DeleteSession(int id)
    {
        try
        {
            var success = await GameSessionService.DeleteGameSessionAsync(id);
            if (success)
            {
                await LoadSessionsAsync();
            }
            else
            {
                ErrorMessage = "Failed to delete game session. It may have already been removed.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting game session: {ex.Message}";
        }
    }

    // Modal Trigger Handlers
    protected void OpenSessionLayersModal(GameSession session)
    {
        SelectedSession = session;
        ShowLayersModal = true;
    }

    protected void CloseSessionLayersModal()
    {
        ShowLayersModal = false;
        SelectedSession = null;
    }

    protected void OpenSessionTeamsModal(GameSession session)
    {
        SelectedSession = session;
        ShowTeamsModal = true;
    }

    protected void CloseSessionTeamsModal()
    {
        ShowTeamsModal = false;
        SelectedSession = null;
    }

    public void Dispose()
    {
        Notifier.OnGameSessionStateChanged -= HandleSessionStateChangedAsync;
    }
}
