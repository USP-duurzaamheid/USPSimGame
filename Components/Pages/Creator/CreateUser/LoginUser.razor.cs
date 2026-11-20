using System.Transactions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Models.LoginDto;

namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class LoginUser(SignInManager<ApplicationUser> signInManager, NavigationManager navigationManager, ILogger<LoginUser> logger)
  {
    private bool _isBusy = false;
    protected string? ErrorMessage { get; set; }
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly ILogger<LoginUser> _logger = logger;

    [CascadingParameter]
    private HttpContext? Context { get; set; }

    [SupplyParameterFromForm(FormName = "login")]
    private LoginDto? Dto { get; set; }


    public async Task SignIn()
    {
      Context!.User.Identities.Select(i => i.AuthenticationType).Where(t => t is not null).ToList().ForEach(async s => await Context.SignOutAsync(s));


      if (_isBusy) return;
      Interlocked.Exchange(ref _isBusy, true);

      try
      {
        var resu = await _signInManager.PasswordSignInAsync(Dto!.UserName, Dto.Password, isPersistent: false, lockoutOnFailure: false);

        if (resu.Succeeded) _navigationManager.NavigateTo("/creator");
        else ErrorMessage = "Could not Authenticate user";
      }
      catch (Exception e)
      {
        ErrorMessage = "Oops, something is wrong on our side, please come back on a different occasion :(";
        _logger.LogError($"LOGIN ERROR: {e}");
      }

      _isBusy = false;
    }

    protected override async Task OnInitializedAsync()
    {
      Dto ??= new();



    }


  }
}