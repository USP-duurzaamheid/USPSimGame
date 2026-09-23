using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Enums;
using USPSimGame.Services;

namespace USPSimGame.Components.Game;

public partial class GameOverlay : ComponentBase, IDisposable
{
    [Inject]
    public PlayerSessionState PlayerSessionState { get; set; } = default!;

    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Inject]
    public IGameSessionNotifierService Notifier { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    protected bool ShowStateControlPanel { get; set; } = false;
    protected bool ShowKpiChartModal { get; set; } = false;
    protected int DurationMinutes { get; set; } = 2;
    protected int DurationSeconds { get; set; } = 0;

    private System.Threading.Timer? _clientTimer;

    public string RemainingTimeFormatted
    {
        get
        {
            var session = PlayerSessionState.CurrentGameSession;
            if (session == null) return string.Empty;

            if (session.State == GameState.Play && session.TargetMonthEndUtc.HasValue)
            {
                var remaining = session.TargetMonthEndUtc.Value - DateTime.UtcNow;
                if (remaining.TotalSeconds <= 0) return "00:00";
                return $"{((int)remaining.TotalMinutes):D2}:{remaining.Seconds:D2}";
            }
            else if (session.State == GameState.Pause && session.RemainingSecondsOnPause.HasValue)
            {
                int sec = session.RemainingSecondsOnPause.Value;
                int m = sec / 60;
                int s = sec % 60;
                return $"{m:D2}:{s:D2}";
            }
            else if (session.State == GameState.Simulation)
            {
                return "Simulating...";
            }

            return string.Empty;
        }
    }

    public string FormattedGameDate
    {
        get
        {
            var session = PlayerSessionState.CurrentGameSession;
            if (session == null)
            {
                return string.Empty;
            }

            int startYear = session.StartYear > 0 ? session.StartYear : 2026;
            return USPSimGame.Utils.CommonGameUtils.FormatMonthYear(session.CurrentMonth, startYear);
        }
    }

    protected override void OnInitialized()
    {
        Notifier.OnGameSessionStateChanged += HandleSessionStateChangedAsync;
        InitDurationInputs();

        // Client-side timer tick every second for smooth display without SignalR traffic
        _clientTimer = new System.Threading.Timer(_ =>
        {
            InvokeAsync(StateHasChanged);
        }, null, 1000, 1000);
    }

    private void InitDurationInputs()
    {
        var session = PlayerSessionState.CurrentGameSession;
        if (session != null)
        {
            int totalSec = session.MonthDurationSeconds > 0 ? session.MonthDurationSeconds : 120;
            DurationMinutes = totalSec / 60;
            DurationSeconds = totalSec % 60;
        }
    }

    private async Task HandleSessionStateChangedAsync(USPSimGame.Data.Entities.GameSession updatedSession)
    {
        if (PlayerSessionState.CurrentGameSession?.Id == updatedSession.Id)
        {
            PlayerSessionState.CurrentGameSession.State = updatedSession.State;
            PlayerSessionState.CurrentGameSession.CurrentMonth = updatedSession.CurrentMonth;
            PlayerSessionState.CurrentGameSession.MonthDurationSeconds = updatedSession.MonthDurationSeconds;
            PlayerSessionState.CurrentGameSession.TargetMonthEndUtc = updatedSession.TargetMonthEndUtc;
            PlayerSessionState.CurrentGameSession.RemainingSecondsOnPause = updatedSession.RemainingSecondsOnPause;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected void ToggleStateControlPanel()
    {
        if (PlayerSessionState.IsAdmin)
        {
            InitDurationInputs();
            ShowStateControlPanel = !ShowStateControlPanel;
        }
    }

    protected void ToggleKpiChartModal()
    {
        ShowKpiChartModal = !ShowKpiChartModal;
    }

    protected async Task ApplyDurationAsync()
    {
        var session = PlayerSessionState.CurrentGameSession;
        if (session != null && PlayerSessionState.IsAdmin)
        {
            int totalSec = (DurationMinutes * 60) + DurationSeconds;
            if (totalSec <= 0) totalSec = 120;

            session.MonthDurationSeconds = totalSec;
            await GameSessionService.UpdateGameSessionStateWithTimerAsync(session.Id, session.State, totalSec);
        }
    }

    protected async Task ChangeGameStateAsync(GameState newState)
    {
        var session = PlayerSessionState.CurrentGameSession;
        if (session != null && PlayerSessionState.IsAdmin)
        {
            int totalSec = (DurationMinutes * 60) + DurationSeconds;
            if (totalSec <= 0) totalSec = 120;

            session.MonthDurationSeconds = totalSec;
            session.State = newState;

            await GameSessionService.UpdateGameSessionStateWithTimerAsync(session.Id, newState, totalSec);
            ShowStateControlPanel = false;
        }
    }

    protected void Disconnect()
    {
        PlayerSessionState.ClearSession();
        Navigation.NavigateTo("/");
    }

    public void Dispose()
    {
        Notifier.OnGameSessionStateChanged -= HandleSessionStateChangedAsync;
        _clientTimer?.Dispose();
    }
}
