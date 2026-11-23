using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using USPSimGame.Services.Teams;
using USPSimGame.Tests.Helpers;

namespace USPSimGame.Tests.Services.Teams;

public class TeamServiceTests
{
    [Fact]
    public async Task ImportTeamPreset_OldPresetWithPasswordHash_StillImports()
    {
        //Arrange: a preset written before team passwords were removed still has a PasswordHash field.
        var dbFactory = new TestDbContextFactory();
        // The notifier is not used by ImportTeamPresetAsync, so null! is safe here.
        var service = new TeamService(dbFactory, null!, NullLogger<TeamService>.Instance);

        const string legacyJson = """
            [
                { "Name": "UU", "Color": "#ffcd00", "PasswordHash": "AQAAAAIAAYagAAAAEold==", "AnnualBudgetAllowance": 100 },
                { "Name": "HU", "Color": "#e6302b", "PasswordHash": "AQAAAAIAAYagAAAAEold==" }
            ]
            """;

        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, legacyJson);

            //Act
            var result = await service.ImportTeamPresetAsync(gameSessionId: 7, path);

            //Assert
            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(2, result.ImportedCount);

            await using var db = await dbFactory.CreateDbContextAsync();
            var teams = await db.Teams.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
            Assert.Equal(["HU", "UU"], teams.Select(t => t.Name));
            Assert.All(teams, t => Assert.Equal(7, t.GameSessionId));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
