using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Pages;

public partial class Join : ComponentBase
{
    protected const int PinLength = 4;

    // TODO: replace with a real game session lookup once the backend supports joining by PIN.
    private static readonly HashSet<string> MockActivePins = ["1234"];

    private ElementReference pinInput;

    protected string Pin { get; set; } = string.Empty;
    protected bool IsFocused { get; set; }
    protected bool IsJoining { get; set; }
    protected string? ErrorMessage { get; set; }
    protected string? SuccessMessage { get; set; }

    protected bool IsPinComplete => Pin.Length == PinLength;
    protected int ActiveSlotIndex => Math.Min(Pin.Length, PinLength - 1);

    private static string Version => typeof(Join).Assembly.GetName().Version?.ToString(3) ?? "Unknown";

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
        SuccessMessage = null;

        if (digits != raw)
        {
            // Render the raw value first so Blazor's diff sees a change and resets the DOM value.
            Pin = raw;
            StateHasChanged();
            await Task.Yield();
        }

        Pin = digits;
    }

    protected async Task HandleJoinAsync()
    {
        if (!IsPinComplete || IsJoining)
        {
            return;
        }

        IsJoining = true;
        ErrorMessage = null;
        SuccessMessage = null;

        // Mocked network round-trip.
        await Task.Delay(600);

        if (MockActivePins.Contains(Pin))
        {
            SuccessMessage = "Game found! Joining...";
        }
        else
        {
            ErrorMessage = "No active game found with this PIN.";
        }

        IsJoining = false;
    }
}
