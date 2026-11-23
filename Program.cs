using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using USPSimGame.Components;
using USPSimGame.Data;
using USPSimGame.Services;
using USPSimGame.Services.Auth;
using USPSimGame.Services.PlayerSessions;
using USPSimGame.Services.GameSessions;
using USPSimGame.Services.Teams;
using USPSimGame.Services.Plans;
using USPSimGame.Services.Costing;
using USPSimGame.Services.Layers;
using USPSimGame.Data.Seeding;
using Microsoft.AspNetCore.Identity;
using USPSimGame.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using USPSimGame.Data.Models;
using System.Security.Claims;
using USPSimGame.Components.Pages;
using System.Net;
using Microsoft.EntityFrameworkCore.Internal;


var builder = WebApplication.CreateBuilder(args);




// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50 MB

    });

builder.Services.AddServerSideBlazor().AddCircuitOptions(options => options.DetailedErrors = true);

builder.Services.AddBlazorBootstrap();

// Add Entity Framework Core & PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));


// Setup UserManager and connect layer 1 & 2
builder.Services
.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

// builder.Services.AddIdentity<ApplicationUser, IdentityRole>();

// Add Identity Services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "AppOrPlayer";
    options.DefaultAuthenticateScheme = "AppOrPlayer";
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddPolicyScheme("AppOrPlayer", "Application or Player", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        if (context.Request.Cookies.ContainsKey("PlayerSession"))
        {
            return "PlayerSession";
        }

        return IdentityConstants.ApplicationScheme;
    };
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/login-user";
    options.Cookie.HttpOnly = true;
    options.AccessDeniedPath = "/Error";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
})
.AddCookie("PlayerSession", options =>
{
    options.LoginPath = "/";
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "PlayerSession";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});


builder.Services.AddAuthorization(); // Add authentication DI to resolve auth states throughout the application
builder.Services.AddCascadingAuthenticationState();

// Bad article: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0&tabs=visual-studio

//Good article: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0

// Register Application Services
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 600000);
builder.Services.AddSingleton<IPasswordHasher, PasswordHasherService>();
// Sets 

builder.Services.AddSingleton<IGameSessionNotifierService, GameSessionNotifierService>();
builder.Services.AddSingleton<ITeamNotifierService, TeamNotifierService>();
builder.Services.AddSingleton<IPlanNotifierService, PlanNotifierService>();
builder.Services.AddSingleton<IJoinCodeGenerator, JoinCodeGenerator>();
builder.Services.AddSingleton<JoinCodeAllocator>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IJoinAttemptLimiter, JoinAttemptLimiter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClientConnectionInfo>();
builder.Services.AddScoped<CircuitHandler, ClientConnectionCircuitHandler>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IPlayerSessionService, PlayerSessionService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IPlanApprovalEvaluationService, PlanApprovalEvaluationService>();
builder.Services.AddScoped<ICostCalculationService, CostCalculationService>();
builder.Services.AddScoped<ITeamBudgetService, TeamBudgetService>();
builder.Services.AddScoped<USPSimGame.Services.Simulation.ISimulationOrchestratorService, USPSimGame.Services.Simulation.SimulationOrchestratorService>();
builder.Services.AddScoped<USPSimGame.Services.Simulation.IKpiChartDataService, USPSimGame.Services.Simulation.KpiChartDataService>();
builder.Services.AddSingleton<USPSimGame.Services.Simulation.ISimulatorModule, USPSimGame.Services.Simulation.Modules.SampleEnergySimulatorModule>();
builder.Services.AddHostedService<USPSimGame.Services.Simulation.GameLoopBackgroundService>();
builder.Services.AddScoped<CreatorAuthState>(); // Per scoped circuit, when on creator creates a legacy creatorAuthState which gets used through components.
builder.Services.AddScoped<PlayerSessionState>();

// Register Building Service & Map Layer Strategy Providers
builder.Services.AddHttpClient<BuildingService>();
builder.Services.AddTransient<IBuildingService>(sp => sp.GetRequiredService<BuildingService>());
builder.Services.AddTransient<IMapLayerProvider>(sp => sp.GetRequiredService<BuildingService>());

builder.Services.AddHttpClient<LianderElektraService>();
builder.Services.AddTransient<IMapLayerProvider>(sp => sp.GetRequiredService<LianderElektraService>());

builder.Services.AddScoped<StedinElektraService>();
builder.Services.AddTransient<IMapLayerProvider>(sp => sp.GetRequiredService<StedinElektraService>());

builder.Services.AddHttpClient<PdokSewageWfsService>();
builder.Services.AddTransient<IMapLayerProvider>(sp => sp.GetRequiredService<PdokSewageWfsService>());

builder.Services.AddHttpClient<PdokBestuurlijkeGebiedenWfsService>();
builder.Services.AddTransient<IMapLayerProvider>(sp => sp.GetRequiredService<PdokBestuurlijkeGebiedenWfsService>());

builder.Services.AddHttpClient<PdokKadastraleKaartWfsService>();
builder.Services.AddTransient<IMapLayerProvider>(sp => sp.GetRequiredService<PdokKadastraleKaartWfsService>());

builder.Services.AddScoped<IMapLayerService, MapLayerService>();

var app = builder.Build();

// Automatically apply EF Core database migrations at application startup with retry loop
using (var scope = app.Services.CreateScope())
{

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    int maxRetries = 10;
    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            logger.LogInformation("Applying EF Core database migrations (attempt {Attempt}/{MaxRetries})...", retry, maxRetries);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("EF Core database migrations applied successfully.");
            await Seeder.AsyncRoles(roleManager, userManager);

            break;
        }
        catch (Exception ex)
        {
            if (ex.ToString().Contains("42P07") || ex.ToString().Contains("already exists"))
            {
                logger.LogWarning("Existing database schema is incompatible with reset EF migrations (table already exists). Re-creating database...");
                try
                {
                    await dbContext.Database.EnsureDeletedAsync();
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("EF Core database successfully re-created and migrated.");
                    break;
                }
                catch (Exception reEx)
                {
                    logger.LogError(reEx, "Failed to re-create database after migration reset.");
                }
            }

            logger.LogError(ex, "Database migration failed on attempt {Attempt}/{MaxRetries}.", retry, maxRetries);
            if (retry == maxRetries)
            {
                logger.LogCritical("Could not apply database migrations after {MaxRetries} attempts.", maxRetries);
                throw;
            }
            await Task.Delay(2000);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();


// Minimal API Endpoint to stream pre-cached layer GeoJSON payloads directly to browser
app.MapGet("/api/layers/{sessionId:int}/{layerKey}", async (int sessionId, string layerKey, AppDbContext db) =>
{
    var layer = await db.GameSessionMapLayers
        .Include(l => l.LayerDefinition)
        .FirstOrDefaultAsync(l => l.GameSessionId == sessionId && l.LayerDefinition.Key == layerKey && l.IsEnabled);

    if (layer == null || string.IsNullOrEmpty(layer.CachedDataContent))
    {
        return Results.NotFound();
    }

    return Results.Content(layer.CachedDataContent, "application/json");
}); // Stedin stuff

app.MapGet("/api/teams/session/{sessionId:int}", async (int sessionId, ITeamService teamService) =>
{
    var teams = await teamService.GetTeamsByGameSessionAsync(sessionId);
    var payload = teams.Select(t => new { id = t.Id, name = t.Name, color = t.Color, areaDefinition = t.AreaDefinition });
    return Results.Ok(payload);
});

app.MapGet("/api/layers/{sessionId:int}/implemented-features", async (int sessionId, int? targetMonth, IPlanService planService) =>
{
    var geoJson = await planService.GetImplementedFeaturesGeoJsonAsync(sessionId, targetMonth);
    return Results.Content(geoJson, "application/json");
});

app.MapPost("/user-logout", async (HttpContext context) =>
{
    // A null check on context.User in unnecessary because it's needed to authorisze the request, this doesn't get executed if there is no context.
    if (context.User.IsInRole("Creator") || context.User.IsInRole("Player")) await context.SignOutAsync(IdentityConstants.ApplicationScheme);
    return Results.Redirect("/");



}).RequireAuthorization();

app.MapPost("/api/join/{joinCode}", async (string joinCode, JoinRequest JoinRequest, HttpContext context, IDbContextFactory<AppDbContext> dbContextFactory, IPlayerSessionService playerSessionService) =>
{
    var canJoin = await playerSessionService.CheckLobby(joinCode, JoinRequest.TeamId, JoinRequest.UserName, context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    if (!canJoin.Success) return Results.BadRequest(new { succes = false });

    using var dbContext = await dbContextFactory.CreateDbContextAsync();
    var result = await PlayerSessionService.AddPlayerSessionAsync(dbContext, JoinRequest.TeamId, JoinRequest.UserName, joinCode); // Add session
    if (!result.Succes) return Results.Ok(new { succes = false });

    context.User.Identities.Select(i => i.AuthenticationType).Where(t => t is not null).ToList().ForEach(async s => await context.SignOutAsync(s)); // Delete present cookies.

    var Claims = new List<Claim>()
    {
        new ("GameID", joinCode),
        new ("TeamID", JoinRequest.TeamId.ToString()),
        new (ClaimTypes.NameIdentifier, result.Player!.Id!),
        new (ClaimTypes.Name, JoinRequest.UserName),
        new (ClaimTypes.Role, "Player"),
    };
    var claimIdentity = new ClaimsIdentity(claims: Claims, authenticationType: "PlayerSession");

    await context.SignInAsync("PlayerSession", new ClaimsPrincipal(claimIdentity), new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });

    return Results.Ok(new { succes = true });
});

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();


app.UseAntiforgery(); // Anti forgery cookie needs to be set later in the middleware pipeline then both auth pipelines

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.Run();
