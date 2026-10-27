using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Models.LoginDto;

namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class LoginUser(SignInManager<ApplicationUser> signInManager, NavigationManager navigationManager)
  {
    private List<string> SubmitMessage { get; set; } = new();
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private bool LoginBool { get; set; } = false;
    private readonly NavigationManager _navigationManager = navigationManager;

    private EditContext? editContext;

    [SupplyParameterFromForm(FormName = "login")]
    private LoginDto? Dto { get; set; }


    public async Task SignIn()
    {
      Console.WriteLine(editContext!.Validate());
      Console.WriteLine(Dto!.Password);
      Console.WriteLine(Dto.UserName);

      if (editContext.Validate())
      {


        var resu = await _signInManager.PasswordSignInAsync(Dto.UserName, Dto.Password, isPersistent: false, false);
        Console.WriteLine(resu.Succeeded);
        Console.WriteLine(resu.IsNotAllowed);



        _navigationManager.NavigateTo("/secured");
      }

    }


    protected override async Task OnInitializedAsync()
    {
      Dto ??= new();
      editContext = new EditContext(Dto);


    }




  }
}