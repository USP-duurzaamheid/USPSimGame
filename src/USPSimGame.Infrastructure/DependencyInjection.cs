using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USPSimGame.Application.Data;
using USPSimGame.Application.Services;
using USPSimGame.Application.Services.Layers;
using USPSimGame.Infrastructure.Identity;
using USPSimGame.Infrastructure.Layers;

namespace USPSimGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                       npgsql.MigrationsAssembly(typeof(InfrastructureMarker).Assembly.FullName))
                   .ConfigureWarnings(warnings =>
                       warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<StedinElektraService>();
        services.AddTransient<IMapLayerProvider>(provider =>
            provider.GetRequiredService<StedinElektraService>());

        return services;
    }
}
