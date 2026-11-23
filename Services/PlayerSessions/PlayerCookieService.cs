
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Models;

namespace USPSimGame.Services.PlayerSessions;

public static class PlayerCookieService
{
  public static async Task SetPlayerCookie(string joinCode, JoinRequest JoinRequest, HttpContext context, (bool Succes, PlayerSession? Player) result)
  {
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
  }
}