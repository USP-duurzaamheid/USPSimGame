using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public interface ITeamService
{
    Task<List<Team>> GetTeamsByGameSessionAsync(int gameSessionId);
    Task<Team?> GetTeamByIdAsync(int id);
    Task<Team> CreateTeamAsync(Team team, string plainPassword);
    Task<Team> UpdateTeamAsync(Team team, string? newPlainPassword);
    Task<bool> DeleteTeamAsync(int id);

    Task<(bool Success, string? ErrorMessage)> TryLockTeamAreaAsync(int teamId, int playerSessionId);
    Task UnlockTeamAreaAsync(int teamId);
    Task UpdateTeamAreaAsync(int teamId, string? areaDefinition);

    Task<List<(string FilePath, string DisplayName)>> GetAvailableTeamPresetsAsync();
    Task<(bool Success, string? ErrorMessage, int ImportedCount)> ImportTeamPresetAsync(int gameSessionId, string filePath);
    Task<(bool Success, string? ErrorMessage, string PresetName)> ExportTeamPresetAsync(int gameSessionId, string presetName);
}
