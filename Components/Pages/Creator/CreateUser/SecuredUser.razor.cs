using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.VisualBasic;

namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class SecuredUser()
  {
    [Inject]
    private AuthenticationStateProvider? AuthState { get; set; }

    public string? UserName { get; set; }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {


      var auth = await AuthState!.GetAuthenticationStateAsync();

      Console.WriteLine($"Authenticated: {auth.User.Identity?.IsAuthenticated}");
      Console.WriteLine($"Authentication Type: {auth.User.Identity?.AuthenticationType}");

      foreach (var claim in auth.User.Claims)
      {
        Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
      }

      UserName = auth.User.FindFirstValue(ClaimTypes.Name);
    }



  }
}