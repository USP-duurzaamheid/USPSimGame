
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Models;

namespace USPSimGame.Services.PlayerSessions;

public static class PlayerCookieService
{
  public static async Task SetCookie(string joinCode, JoinRequest JoinRequest, HttpContext context, (bool Succes, PlayerSession? Player) result)
  {
    List<string?> schemas = context.User.Identities.Select(i => i.AuthenticationType).Where(t => t is not null).ToList(); // Delete present cookies.

    foreach (string? schema in schemas) // Use PLINQ for async foreach
    {
      if (schema is null) continue;
      await context.SignOutAsync(schema);
    }

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
  }


}