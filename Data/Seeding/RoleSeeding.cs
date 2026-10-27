using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace USPSimGame.Data.Seeding;


public class Seeder
{
  public static async Task AsyncRoles(RoleManager<IdentityRole> roleManager)
  {

    string[] roles = { "Creator", "Player", "TestRole" };

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
}