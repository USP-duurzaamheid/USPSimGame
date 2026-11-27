using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using USPSimGame.Data;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Seeding;

namespace USPSimGame.Tests.Data.Seeding;

public class SeederTests
{
    // Same Identity setup as Program.cs, on its own InMemory database.
    private static ServiceProvider BuildIdentityServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task AsyncRoles_FreshDatabase_CreatesAdminWithCreatorRole()
    {
        //Arrange
        using var provider = BuildIdentityServices();
        using var scope = provider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //Act
        await Seeder.AsyncRoles(roleManager, userManager);

        //Assert
        var admin = await userManager.FindByNameAsync("admin");
        Assert.NotNull(admin);
        Assert.True(await userManager.IsInRoleAsync(admin, "Creator"));
        Assert.False(string.IsNullOrEmpty(admin.PasswordHash));
    }

    [Fact]
    public async Task AsyncRoles_RunTwice_KeepsOneAdmin()
    {
        //Arrange: the seeder runs on every app start.
        using var provider = BuildIdentityServices();
        using var scope = provider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //Act
        await Seeder.AsyncRoles(roleManager, userManager);
        await Seeder.AsyncRoles(roleManager, userManager);

        //Assert
        Assert.Single(userManager.Users.Where(u => u.UserName == "admin"));
    }
}
