using Microsoft.AspNetCore.Components;

namespace USPSimGame.Components.Pages;

public partial class Join : ComponentBase
{
    protected const int PinLength = 4;
    private const string MockPin = "1234";

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

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

    protected void HandleJoin()
    {
        if (Pin == MockPin)
        {
            Navigation.NavigateTo("/join/team");
            return;
        }

        ErrorMessage = "No active game found with this PIN.";
    }
}
