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
          var createdRole = await roleManager.CreateAsync(IDRole);
          var associatedClaim = await roleManager.AddClaimAsync(IDRole, new Claim("Role", role));

          if (!createdRole.Succeeded || !associatedClaim.Succeeded) Console.WriteLine("Configured role or claim incorrectly added");
        }
      }

    }
    catch (Exception e)
    {
      Console.WriteLine($"Can't seed roles: ${e}");
    }


    var testUser = new ApplicationUser { UserName = "admin", PasswordHash = "AQAAAAIAAYagAAAAEITB3XoBD9OUNfCEFQjdACjOmvavSu6DAGcOCZui+ELzkJlD0TPulJwrfEH7Bdajfg==", Email = "testmail@gmail.com", Id = "1" };
    var fetchUser = await userManager.FindByIdAsync(testUser.Id);

    if (fetchUser is not null && fetchUser.Id == testUser.Id) return;

    try
    {
      var createdUser = await userManager.CreateAsync(testUser);
      var associatedRole = await userManager.AddToRoleAsync(testUser, "Creator");
      if (!createdUser.Succeeded || !associatedRole.Succeeded) Console.WriteLine($"Test user incorrectly created. ID:  {testUser.Id}");
    }
    catch (Exception e)
    {
      Console.WriteLine($"Can't seed test user: ${e}");
    }



  }
}