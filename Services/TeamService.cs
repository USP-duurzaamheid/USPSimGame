using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;
using USPSimGame.Data.Entities;

namespace USPSimGame.Services;

public class TeamService : ITeamService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITeamNotifierService _teamNotifier;
    private readonly ILogger<TeamService> _logger;

    public TeamService(IDbContextFactory<AppDbContext> dbContextFactory, IPasswordHasher passwordHasher, ITeamNotifierService teamNotifier, ILogger<TeamService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _passwordHasher = passwordHasher;
        _teamNotifier = teamNotifier;
        _logger = logger;
    }

    public async Task<List<Team>> GetTeamsByGameSessionAsync(int gameSessionId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var teams = await db.Teams
            .Where(t => t.GameSessionId == gameSessionId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var lockedSessionIds = teams
            .Where(t => !string.IsNullOrEmpty(t.LockedBySessionId) && int.TryParse(t.LockedBySessionId, out _))
            .Select(t => int.Parse(t.LockedBySessionId!))
            .Distinct()
            .ToList();

        if (lockedSessionIds.Any())
        {
            var playerSessions = await db.PlayerSessions
                .Where(ps => lockedSessionIds.Contains(ps.Id))
                .ToDictionaryAsync(ps => ps.Id.ToString(), ps => ps.UserName);

            foreach (var team in teams)
            {
                if (!string.IsNullOrEmpty(team.LockedBySessionId) && playerSessions.TryGetValue(team.LockedBySessionId, out var userName))
                {
                    team.LockedByUserName = userName;
                }
            }
        }

        return teams;
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var team = await db.Teams.FindAsync(id);
        if (team != null && !string.IsNullOrEmpty(team.LockedBySessionId) && int.TryParse(team.LockedBySessionId, out int playerSessionId))
        {
            var ps = await db.PlayerSessions.FindAsync(playerSessionId);
            if (ps != null)
            {
                team.LockedByUserName = ps.UserName;
            }
        }
        return team;
    }

    public async Task<Team> CreateTeamAsync(Team team, string plainPassword)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        team.PasswordHash = _passwordHasher.HashPassword(plainPassword);
        if (string.IsNullOrWhiteSpace(team.Color))
        {
            team.Color = "#3b82f6";
        }

        db.Teams.Add(team);
        await db.SaveChangesAsync();
        return team;
    }

    public async Task<Team> UpdateTeamAsync(Team team, string? newPlainPassword)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var existing = await db.Teams.FindAsync(team.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Team with ID {team.Id} was not found.");
        }

        existing.Name = team.Name;
        existing.Color = string.IsNullOrWhiteSpace(team.Color) ? "#3b82f6" : team.Color;
        existing.AnnualBudgetAllowance = team.AnnualBudgetAllowance;

        if (!string.IsNullOrEmpty(newPlainPassword))
        {
            existing.PasswordHash = _passwordHasher.HashPassword(newPlainPassword);
        }

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var team = await db.Teams.FindAsync(id);
        if (team == null)
        {
            return false;
        }

        db.Teams.Remove(team);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? ErrorMessage)> TryLockTeamAreaAsync(int teamId, int playerSessionId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var team = await db.Teams.FindAsync(teamId);

        if (team == null)
        {
            return (false, "Team not found.");
        }

        string sessionStr = playerSessionId.ToString();

        if (!string.IsNullOrEmpty(team.LockedBySessionId) && team.LockedBySessionId != sessionStr)
        {
            var lockingPlayer = await db.PlayerSessions.FindAsync(int.Parse(team.LockedBySessionId));
            string name = lockingPlayer?.UserName ?? "another player";
            return (false, $"Team area is currently being defined by team member {name}.");
        }

        team.LockedBySessionId = sessionStr;
        team.LockedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await _teamNotifier.NotifyTeamAreaChangedAsync(team.GameSessionId);

        return (true, null);
    }

    public async Task UnlockTeamAreaAsync(int teamId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var team = await db.Teams.FindAsync(teamId);

        if (team != null && !string.IsNullOrEmpty(team.LockedBySessionId))
        {
            int sessionId = team.GameSessionId;
            team.LockedBySessionId = null;
            team.LockedAt = null;
            await db.SaveChangesAsync();

            await _teamNotifier.NotifyTeamAreaChangedAsync(sessionId);
        }
    }

    public async Task UpdateTeamAreaAsync(int teamId, string? areaDefinition)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var team = await db.Teams.FindAsync(teamId);

        if (team != null)
        {
            team.AreaDefinition = areaDefinition;
            await db.SaveChangesAsync();

            await _teamNotifier.NotifyTeamAreaChangedAsync(team.GameSessionId);
        }
    }

    public async Task<List<(string FilePath, string DisplayName)>> GetAvailableTeamPresetsAsync()
    {
        var presetsDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Teams");
        if (!Directory.Exists(presetsDir))
        {
            return new List<(string, string)>();
        }

        var files = Directory.GetFiles(presetsDir, "*.json");
        var result = new List<(string FilePath, string DisplayName)>();

        foreach (var file in files)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            string displayName = filename.Replace("-", " ").Replace("_", " ");
            result.Add((file, displayName));
        }

        return await Task.FromResult(result.OrderBy(r => r.DisplayName).ToList());
    }

    public async Task<(bool Success, string? ErrorMessage, int ImportedCount)> ImportTeamPresetAsync(int gameSessionId, string filePath)
    {
        if (!File.Exists(filePath))
        {
            return (false, "Preset file not found.", 0);
        }

        string jsonContent = await File.ReadAllTextAsync(filePath);
        var (isValid, teams, errorMsg) = ValidateAndParseTeamPresetJson(jsonContent);

        if (!isValid || teams == null || !teams.Any())
        {
            return (false, errorMsg ?? "Invalid JSON preset format.", 0);
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        int importedCount = 0;

        foreach (var team in teams)
        {
            team.Id = 0; // Fresh DB identity insertion
            team.GameSessionId = gameSessionId;
            if (string.IsNullOrWhiteSpace(team.Color))
            {
                team.Color = "#3b82f6";
            }
            db.Teams.Add(team);
            importedCount++;
        }

        await db.SaveChangesAsync();
        _logger.LogInformation("TeamService: Imported {Count} preset teams from '{File}' into GameSession #{SessionId}", importedCount, Path.GetFileName(filePath), gameSessionId);
        return (true, null, importedCount);
    }

    public static (bool IsValid, List<Team>? Teams, string? ErrorMessage) ValidateAndParseTeamPresetJson(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return (false, null, "JSON content is empty.");
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(jsonContent);
            if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return (false, null, "JSON root must be an array of team objects.");
            }

            var teams = new List<Team>();
            int index = 0;

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                index++;
                if (element.ValueKind != System.Text.Json.JsonValueKind.Object)
                {
                    return (false, null, $"Item #{index} in JSON array is not a valid JSON object.");
                }

                if (!element.TryGetProperty("Name", out var nameProp) || string.IsNullOrWhiteSpace(nameProp.GetString()))
                {
                    return (false, null, $"Item #{index} is missing a valid 'Name' property.");
                }

                if (!element.TryGetProperty("Color", out var colorProp) || string.IsNullOrWhiteSpace(colorProp.GetString()))
                {
                    return (false, null, $"Item #{index} ('{nameProp.GetString()}') is missing a valid 'Color' hex string.");
                }

                if (!element.TryGetProperty("PasswordHash", out var passHashProp) || string.IsNullOrWhiteSpace(passHashProp.GetString()))
                {
                    return (false, null, $"Item #{index} ('{nameProp.GetString()}') is missing a valid 'PasswordHash' property.");
                }

                string? areaDefString = null;
                if (element.TryGetProperty("AreaDefinition", out var areaDefProp))
                {
                    if (areaDefProp.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        areaDefString = areaDefProp.GetString();
                    }
                    else if (areaDefProp.ValueKind == System.Text.Json.JsonValueKind.Object || areaDefProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        areaDefString = areaDefProp.GetRawText();
                    }

                    if (!string.IsNullOrWhiteSpace(areaDefString))
                    {
                        try
                        {
                            using var areaDoc = System.Text.Json.JsonDocument.Parse(areaDefString);
                        }
                        catch (Exception)
                        {
                            return (false, null, $"Item #{index} ('{nameProp.GetString()}') contains invalid GeoJSON in 'AreaDefinition'.");
                        }
                    }
                }

                double annualBudgetAllowance = 100;
                if (element.TryGetProperty("AnnualBudgetAllowance", out var budgetProp) && budgetProp.TryGetDouble(out var parsedBudget))
                {
                    annualBudgetAllowance = parsedBudget;
                }

                var team = new Team
                {
                    Name = nameProp.GetString()!.Trim(),
                    Color = colorProp.GetString()!.Trim(),
                    PasswordHash = passHashProp.GetString()!.Trim(),
                    AreaDefinition = areaDefString,
                    AnnualBudgetAllowance = annualBudgetAllowance
                };

                teams.Add(team);
            }

            if (!teams.Any())
            {
                return (false, null, "JSON preset contains no team definitions.");
            }

            return (true, teams, null);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return (false, null, $"JSON syntax error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"Validation error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage, string PresetName)> ExportTeamPresetAsync(int gameSessionId, string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return (false, "Preset name cannot be empty.", string.Empty);
        }

        string cleanName = presetName.Trim();
        string sanitizedFileName = string.Join("-", cleanName.Split(Path.GetInvalidFileNameChars()))
            .Replace(" ", "-");
        if (!sanitizedFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            sanitizedFileName += ".json";
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var teams = await db.Teams
            .Where(t => t.GameSessionId == gameSessionId)
            .OrderBy(t => t.Id)
            .ToListAsync();

        if (!teams.Any())
        {
            return (false, "No teams defined in this session to export.", string.Empty);
        }

        var exportPayload = teams.Select(t => new
        {
            Name = t.Name,
            Color = t.Color,
            PasswordHash = t.PasswordHash,
            AreaDefinition = t.AreaDefinition,
            AnnualBudgetAllowance = t.AnnualBudgetAllowance
        }).ToList();

        try
        {
            var presetsDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Teams");
            Directory.CreateDirectory(presetsDir);
            string fullPath = Path.Combine(presetsDir, sanitizedFileName);

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = System.Text.Json.JsonSerializer.Serialize(exportPayload, options);
            await File.WriteAllTextAsync(fullPath, json);

            _logger.LogInformation("TeamService: Exported {Count} teams from Session #{SessionId} to preset file '{File}'", teams.Count, sanitizedFileName, gameSessionId);
            return (true, null, Path.GetFileNameWithoutExtension(sanitizedFileName).Replace("-", " "));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TeamService: Error exporting team preset '{Name}'", presetName);
            return (false, $"Error saving preset file: {ex.Message}", string.Empty);
        }
    }
}
