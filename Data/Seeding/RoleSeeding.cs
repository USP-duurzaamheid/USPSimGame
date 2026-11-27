using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using USPSimGame.Data.Entities;


namespace USPSimGame.Data.Seeding;


public class Seeder
{
  public static async Task AsyncRoles(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
  {

    string[] roles = { "Creator", "Player" };

    try
    {
      foreach (string role in roles)
      {
        if (!await roleManager.RoleExistsAsync(role))
        {
          var IDRole = new IdentityRole(role);
          await roleManager.CreateAsync(IDRole);
          await roleManager.AddClaimAsync(IDRole, new Claim("Role", role));
        }
      }
    }
    catch (Exception e)
    {
      Console.WriteLine($"Can't seed roles: {e}");
    }

    // Look the admin up in the database: a fresh database has no Identity users, so without this nobody can reach /creator.
    if (await userManager.FindByNameAsync("admin") != null) return;

    var adminUser = new ApplicationUser { UserName = "admin", PasswordHash = "AQAAAAIAAYagAAAAEITB3XoBD9OUNfCEFQjdACjOmvavSu6DAGcOCZui+ELzkJlD0TPulJwrfEH7Bdajfg==", Email = "testmail@gmail.com" };

    try
    {
      // CreateAsync and AddToRoleAsync report failures in their result instead of throwing.
      var created = await userManager.CreateAsync(adminUser);
      if (!created.Succeeded)
      {
        Console.WriteLine($"Can't seed admin user: {string.Join(", ", created.Errors.Select(err => err.Description))}");
        return;
      }

      var addedToRole = await userManager.AddToRoleAsync(adminUser, "Creator");
      if (!addedToRole.Succeeded)
      {
        Console.WriteLine($"Can't give admin user the Creator role: {string.Join(", ", addedToRole.Errors.Select(err => err.Description))}");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine($"Can't seed admin user: {e}");
    }
  }
}