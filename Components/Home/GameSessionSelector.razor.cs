using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;

namespace USPSimGame.Components.Home;

public partial class GameSessionSelector : ComponentBase, IDisposable
{
    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Inject]
    public IGameSessionNotifierService Notifier { get; set; } = default!;

    [Parameter]
    public EventCallback<GameSession> OnSessionSelected { get; set; }

    [Parameter]
    public int SelectedSessionId { get; set; }

    protected List<GameSession>? GameSessions { get; set; }
    protected bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        Notifier.OnGameSessionStateChanged += HandleSessionStateChangedAsync;
        await LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        IsLoading = true;
        try
        {
            GameSessions = await GameSessionService.GetGameSessionsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSessionStateChangedAsync(GameSession updatedSession)
    {
        await InvokeAsync(LoadSessionsAsync);
    }

    protected async Task SelectSession(GameSession session)
    {
        SelectedSessionId = session.Id;
        await OnSessionSelected.InvokeAsync(session);
    }

    public void Dispose()
    {
        Notifier.OnGameSessionStateChanged -= HandleSessionStateChangedAsync;
    }
}
