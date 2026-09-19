using Microsoft.Extensions.DependencyInjection;
using USPSimGame.Application.Services;
using USPSimGame.Application.Services.Costing;
using USPSimGame.Application.Services.Layers;
using USPSimGame.Application.Services.Plans;
using USPSimGame.Application.Services.Simulation;
using USPSimGame.Application.Services.Simulation.Modules;

namespace USPSimGame.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IGameSessionNotifierService, GameSessionNotifierService>();
        services.AddSingleton<ITeamNotifierService, TeamNotifierService>();
        services.AddSingleton<IPlanNotifierService, PlanNotifierService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGameSessionService, GameSessionService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IPlayerSessionService, PlayerSessionService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IPlanApprovalEvaluationService, PlanApprovalEvaluationService>();
        services.AddScoped<ICostCalculationService, CostCalculationService>();
        services.AddScoped<ITeamBudgetService, TeamBudgetService>();
        services.AddScoped<ISimulationOrchestratorService, SimulationOrchestratorService>();
        services.AddScoped<IKpiChartDataService, KpiChartDataService>();
        services.AddSingleton<ISimulatorModule, SampleEnergySimulatorModule>();
        services.AddHostedService<GameLoopBackgroundService>();
        services.AddScoped<CreatorAuthState>();
        services.AddScoped<PlayerSessionState>();

        services.AddHttpClient<BuildingService>();
        services.AddTransient<IBuildingService>(provider => provider.GetRequiredService<BuildingService>());
        services.AddTransient<IMapLayerProvider>(provider => provider.GetRequiredService<BuildingService>());

        services.AddHttpClient<LianderElektraService>();
        services.AddTransient<IMapLayerProvider>(provider => provider.GetRequiredService<LianderElektraService>());

        services.AddHttpClient<PdokSewageWfsService>();
        services.AddTransient<IMapLayerProvider>(provider => provider.GetRequiredService<PdokSewageWfsService>());

        services.AddHttpClient<PdokBestuurlijkeGebiedenWfsService>();
        services.AddTransient<IMapLayerProvider>(provider => provider.GetRequiredService<PdokBestuurlijkeGebiedenWfsService>());

        services.AddHttpClient<PdokKadastraleKaartWfsService>();
        services.AddTransient<IMapLayerProvider>(provider => provider.GetRequiredService<PdokKadastraleKaartWfsService>());

        services.AddScoped<IMapLayerService, MapLayerService>();

        return services;
    }
}
