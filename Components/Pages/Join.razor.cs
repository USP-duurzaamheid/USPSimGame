using Microsoft.AspNetCore.Components;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.PlayerSessions;

namespace USPSimGame.Components.Pages;

public partial class Join : ComponentBase
{
    protected const int PinLength = JoinCodeGenerator.CodeLength;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    [Inject]
    public IGameSessionService GameSessionService { get; set; } = default!;

    [Inject]
    public ClientConnectionInfo ClientConnection { get; set; } = default!;

    [Inject]
    public ILogger<Join> Logger { get; set; } = default!;

    private ElementReference pinInput;

    protected string Pin { get; set; } = string.Empty;
    protected bool IsFocused { get; set; }
    protected string? ErrorMessage { get; set; }

    protected bool IsPinComplete => Pin.Length == PinLength;
    protected int ActiveSlotIndex => Math.Min(Pin.Length, PinLength - 1);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await pinInput.FocusAsync();
        }
    }

    protected async Task HandlePinInputAsync(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        var digits = new string(raw.Where(char.IsAsciiDigit).Take(PinLength).ToArray());

        ErrorMessage = null;

        if (digits != raw)
        {
            // Render the raw value first so Blazor's diff sees a change and resets the DOM value.
            Pin = raw;
            StateHasChanged();
            await Task.Yield();
        }

        Pin = digits;
    }

    protected async Task HandleJoin()
    {
        // The UI check is only for convenience; FindJoinableByJoinCodeAsync validates again on the server.
        if (!JoinCodeFormat.TryNormalize(Pin, out var code))
        {
            ErrorMessage = $"Please enter a {PinLength}-digit game code.";
            return;
        }

        try
        {
            var result = await GameSessionService.FindJoinableByJoinCodeAsync(code, ClientConnection.ClientKey);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Join: Error while looking up a game session by join code");
            ErrorMessage = "Something went wrong. Please try again.";
            return;
        }

        Navigation.NavigateTo($"/join/team/{code}");
    }
}
