using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using USPSimGame.Data.Entities;
using USPSimGame.Services;

namespace USPSimGame.Components.Game;

public partial class TeamAreaPanel : ComponentBase
{
    [Inject]
    public ITeamService TeamService { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter, EditorRequired]
    public int GameSessionId { get; set; }

    [Parameter, EditorRequired]
    public Team Team { get; set; } = default!;

    [Parameter]
    public int CurrentPlayerSessionId { get; set; }

    protected bool IsEditing { get; set; } = false;
    protected bool IsSaving { get; set; } = false;
    protected string? ErrorMessage { get; set; }

    protected bool IsLockedByOther => !string.IsNullOrEmpty(Team.LockedBySessionId) &&
                                     Team.LockedBySessionId != CurrentPlayerSessionId.ToString();

    protected async Task StartEditingAsync()
    {
        ErrorMessage = null;
        var (success, err) = await TeamService.TryLockTeamAreaAsync(Team.Id, CurrentPlayerSessionId);
        if (!success)
        {
            ErrorMessage = err;
            return;
        }

        IsEditing = true;
        string teamColor = string.IsNullOrWhiteSpace(Team.Color) ? "#3b82f6" : Team.Color;
        string fillColor = HexToRgba(teamColor, 0.35);

        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.startDrawing", "Polygon", teamColor, fillColor, "team_area");
            if (!string.IsNullOrWhiteSpace(Team.AreaDefinition))
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.loadDraftFeatureGeometry", "team_area", Team.AreaDefinition);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeamAreaPanel] Error starting drawing tool: {ex.Message}");
        }
    }

    protected async Task SaveAsync()
    {
        IsSaving = true;
        ErrorMessage = null;
        try
        {
            string? geoJson = await JSRuntime.InvokeAsync<string?>("uspsim2d5.getDrawnGeoJsonForLayer", "team_area");
            await TeamService.UpdateTeamAreaAsync(Team.Id, geoJson);
            await TeamService.UnlockTeamAreaAsync(Team.Id);

            Team.AreaDefinition = geoJson;
            IsEditing = false;

            await JSRuntime.InvokeVoidAsync("uspsim2d5.stopDrawing");
            await JSRuntime.InvokeVoidAsync("uspsim2d5.refreshTeamAreas");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving team area: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected async Task CancelAsync()
    {
        try
        {
            await TeamService.UnlockTeamAreaAsync(Team.Id);
            await JSRuntime.InvokeVoidAsync("uspsim2d5.stopDrawing");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeamAreaPanel] Error stopping drawing tool: {ex.Message}");
        }
        IsEditing = false;
    }

    protected async Task UndoPointAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.undoDrawPoint");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeamAreaPanel] Error invoking undoDrawPoint: {ex.Message}");
        }
    }

    protected async Task RedoPointAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.redoDrawPoint");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeamAreaPanel] Error invoking redoDrawPoint: {ex.Message}");
        }
    }

    protected async Task DeletePointAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("uspsim2d5.deleteSelectedVertex");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeamAreaPanel] Error invoking deleteSelectedVertex: {ex.Message}");
        }
    }

    private static string HexToRgba(string hex, double alpha)
    {
        try
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return $"rgba({r}, {g}, {b}, {alpha.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
            }
        }
        catch { }
        return "rgba(59, 130, 246, 0.35)";
    }
}
