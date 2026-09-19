using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using USPSimGame.Application.Data;

namespace USPSimGame.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<InfrastructureMarker>>();
        const int maxRetries = 10;

        for (var retry = 1; retry <= maxRetries; retry++)
        {
            try
            {
                logger.LogInformation(
                    "Applying EF Core database migrations (attempt {Attempt}/{MaxRetries})...",
                    retry,
                    maxRetries);
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("EF Core database migrations applied successfully.");
                return;
            }
            catch (Exception exception) when (retry < maxRetries)
            {
                logger.LogError(
                    exception,
                    "Database migration failed on attempt {Attempt}/{MaxRetries}.",
                    retry,
                    maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogCritical(
                    exception,
                    "Could not apply database migrations after {MaxRetries} attempts. The database was not modified outside the migration transaction.",
                    maxRetries);
                throw;
            }
        }
    }
}
