using Microsoft.EntityFrameworkCore;
using USPSimGame.Data;

namespace USPSimGame.Tests.Helpers;

/// <summary>
/// Hands out AppDbContexts on one InMemory database. Every factory gets its own database name, so tests never share data.
/// Note: the InMemory provider does not enforce unique indexes or max lengths; that needs a real Postgres.
/// </summary>
public class TestDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDbContextFactory()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    public AppDbContext CreateDbContext() => new(_options);

    // The interface has a default implementation, but default interface methods are not visible on the class itself.
    public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateDbContext());
}
