using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services;

namespace USPSimGame.Components.Creator;

public partial class SessionTeamsModal : ComponentBase
{
    [Inject]
    public ITeamService TeamService { get; set; } = default!;

    [Parameter, EditorRequired]
    public GameSession Session { get; set; } = default!;

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback OnUpdated { get; set; }

    protected List<Team> SessionTeams { get; set; } = new();
    protected List<(string FilePath, string DisplayName)> AvailablePresets { get; set; } = new();
    protected bool IsImportingPreset { get; set; } = false;
    protected string? PresetErrorMessage { get; set; }
    protected string? PresetSuccessMessage { get; set; }

    // Export preset state
    protected bool ShowExportForm { get; set; } = false;
    protected string ExportPresetName { get; set; } = string.Empty;
    protected bool IsExportingPreset { get; set; } = false;

    protected string NewTeamName { get; set; } = string.Empty;
    protected string NewTeamPassword { get; set; } = string.Empty;
    protected string NewTeamColor { get; set; } = "#3b82f6";
    protected int NewTeamAnnualBudgetAllowance { get; set; } = 100;
    protected bool IsLoading { get; set; } = true;

    // Inline edit state
    protected int? EditingTeamId { get; set; }
    protected string EditingTeamName { get; set; } = string.Empty;
    protected string EditingTeamPassword { get; set; } = string.Empty;
    protected string EditingTeamColor { get; set; } = "#3b82f6";
    protected int EditingTeamAnnualBudgetAllowance { get; set; } = 100;

    protected override async Task OnInitializedAsync()
    {
        await LoadSessionTeamsAsync();
        await LoadAvailablePresetsAsync();
    }

    protected async Task LoadAvailablePresetsAsync()
    {
        try
        {
            AvailablePresets = await TeamService.GetAvailableTeamPresetsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionTeamsModal] Error loading team presets: {ex.Message}");
        }
    }

    protected async Task LoadSessionTeamsAsync()
    {
        IsLoading = true;
        try
        {
            SessionTeams = await TeamService.GetTeamsByGameSessionAsync(Session.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionTeamsModal] Error loading teams: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task ImportPresetAsync(string filePath)
    {
        PresetErrorMessage = null;
        PresetSuccessMessage = null;
        IsImportingPreset = true;

        try
        {
            var (success, errorMsg, count) = await TeamService.ImportTeamPresetAsync(Session.Id, filePath);
            if (success)
            {
                PresetSuccessMessage = $"Successfully imported {count} team(s) from preset!";
                await LoadSessionTeamsAsync();
                await OnUpdated.InvokeAsync();
            }
            else
            {
                PresetErrorMessage = errorMsg ?? "Failed to import team preset.";
            }
        }
        catch (Exception ex)
        {
            PresetErrorMessage = $"Error importing preset: {ex.Message}";
        }
        finally
        {
            IsImportingPreset = false;
        }
    }

    protected void ResetNewTeamForm()
    {
        NewTeamName = string.Empty;
        NewTeamPassword = string.Empty;
        NewTeamColor = "#3b82f6";
        NewTeamAnnualBudgetAllowance = 100;
    }

    protected async Task AddTeamToSessionAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewTeamName) && !string.IsNullOrWhiteSpace(NewTeamPassword))
        {
            try
            {
                var newTeam = new Team
                {
                    GameSessionId = Session.Id,
                    Name = NewTeamName.Trim(),
                    Color = string.IsNullOrWhiteSpace(NewTeamColor) ? "#3b82f6" : NewTeamColor.Trim(),
                    AnnualBudgetAllowance = NewTeamAnnualBudgetAllowance
                };

                await TeamService.CreateTeamAsync(newTeam, NewTeamPassword.Trim());
                ResetNewTeamForm();
                await LoadSessionTeamsAsync();
                await OnUpdated.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionTeamsModal] Error creating team: {ex.Message}");
            }
        }
    }

    protected void StartEditTeam(Team team)
    {
        EditingTeamId = team.Id;
        EditingTeamName = team.Name;
        EditingTeamPassword = string.Empty; // Blank means leave existing password unchanged
        EditingTeamColor = team.Color;
        EditingTeamAnnualBudgetAllowance = (int)team.AnnualBudgetAllowance;
    }

    protected void CancelEditTeam()
    {
        EditingTeamId = null;
        EditingTeamName = string.Empty;
        EditingTeamPassword = string.Empty;
        EditingTeamColor = "#3b82f6";
        EditingTeamAnnualBudgetAllowance = 100;
    }

    protected async Task SaveEditedTeamAsync()
    {
        if (EditingTeamId.HasValue && !string.IsNullOrWhiteSpace(EditingTeamName))
        {
            try
            {
                var teamToUpdate = new Team
                {
                    Id = EditingTeamId.Value,
                    GameSessionId = Session.Id,
                    Name = EditingTeamName.Trim(),
                    Color = string.IsNullOrWhiteSpace(EditingTeamColor) ? "#3b82f6" : EditingTeamColor.Trim(),
                    AnnualBudgetAllowance = EditingTeamAnnualBudgetAllowance
                };

                string? passwordArg = string.IsNullOrWhiteSpace(EditingTeamPassword) ? null : EditingTeamPassword.Trim();

                await TeamService.UpdateTeamAsync(teamToUpdate, passwordArg);
                CancelEditTeam();
                await LoadSessionTeamsAsync();
                await OnUpdated.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionTeamsModal] Error updating team: {ex.Message}");
            }
        }
    }

    protected async Task DeleteTeamFromSessionAsync(int teamId)
    {
        try
        {
            if (EditingTeamId == teamId)
            {
                CancelEditTeam();
            }

            await TeamService.DeleteTeamAsync(teamId);
            await LoadSessionTeamsAsync();
            await OnUpdated.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionTeamsModal] Error deleting team: {ex.Message}");
        }
    }

    protected void ToggleExportForm()
    {
        ShowExportForm = !ShowExportForm;
        if (ShowExportForm)
        {
            ExportPresetName = Session.Name;
        }
    }

    protected async Task ExportTeamsToPresetAsync()
    {
        if (string.IsNullOrWhiteSpace(ExportPresetName))
        {
            return;
        }

        PresetErrorMessage = null;
        PresetSuccessMessage = null;
        IsExportingPreset = true;

        try
        {
            var (success, errorMsg, presetName) = await TeamService.ExportTeamPresetAsync(Session.Id, ExportPresetName);
            if (success)
            {
                PresetSuccessMessage = $"Successfully exported team preset '{presetName}' to Data/Teams!";
                ShowExportForm = false;
                await LoadAvailablePresetsAsync();
            }
            else
            {
                PresetErrorMessage = errorMsg ?? "Failed to export team preset.";
            }
        }
        catch (Exception ex)
        {
            PresetErrorMessage = $"Error exporting preset: {ex.Message}";
        }
        finally
        {
            IsExportingPreset = false;
        }
    }

    protected async Task CloseAsync()
    {
        await OnClose.InvokeAsync();
    }
}
