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
      Console.WriteLine($"Can't seed roles: ${e}");
    }


    var testuser = new ApplicationUser { UserName = "admin", PasswordHash = "AQAAAAIAAYagAAAAEITB3XoBD9OUNfCEFQjdACjOmvavSu6DAGcOCZui+ELzkJlD0TPulJwrfEH7Bdajfg==", Email = "testmail@gmail.com", Id = "1" };
    if (await userManager.GetUserIdAsync(testuser) == testuser.Id) return;

    try
    {
      await userManager.CreateAsync(testuser);
      await userManager.AddToRoleAsync(testuser, "Creator");
    }
    catch (Exception e)
    {
      Console.WriteLine($"Can't seed test user: ${e}");
    }



  }
}