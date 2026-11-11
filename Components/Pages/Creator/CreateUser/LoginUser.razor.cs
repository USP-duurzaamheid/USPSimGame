using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Models.LoginDto;

namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class LoginUser(SignInManager<ApplicationUser> signInManager, NavigationManager navigationManager, ILogger<LoginUser> logger)
  {
    protected string? ErrorMessage { get; set; }
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly ILogger<LoginUser> _logger = logger;


    [SupplyParameterFromForm(FormName = "login")]
    private LoginDto? Dto { get; set; }


    public async Task SignIn()
    {

      try
      {
        var resu = await _signInManager.PasswordSignInAsync(Dto!.UserName, Dto.Password, isPersistent: false, false);
        if (resu.Succeeded) _navigationManager.NavigateTo("/creator");
        else ErrorMessage = "Could not Authenticate user";
      }
      catch (Exception e)
      {
        ErrorMessage = "Oops, something is wrong on our side, please come back on a different occasion :(";
        _logger.LogError($"LOGIN ERROR: {e}");
      }


    }

    protected override async Task OnInitializedAsync()
    {
      Dto ??= new();



    }

    public void HandleLoginSuccess()
    {
      Console.WriteLine("Hallo");
    }


  }
}