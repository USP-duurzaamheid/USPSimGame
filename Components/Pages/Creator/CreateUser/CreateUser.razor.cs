using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using USPSimGame.Data.Entities;



namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class CreateUser(UserManager<ApplicationUser> userManager)
  {

    [SupplyParameterFromForm]
    private UserDto Dto { get; set; }
    private List<string> SubmitMessage { get; set; } = [];
    private readonly UserManager<ApplicationUser> _userManager = userManager;




    protected override void OnInitialized()
    {
      Dto ??= new();
    }




    private async Task AddUser()
    {
      SubmitMessage = [];

      if (string.IsNullOrEmpty(Dto.Email) || string.IsNullOrEmpty(Dto.Password) || string.IsNullOrEmpty(Dto.UserName))
      {
        Console.WriteLine("Something is wrong");
        SubmitMessage[0] = "Please enter something in the fields";
        return;
      }

      var resu = await _userManager.CreateAsync(new ApplicationUser(Dto.Email, Dto.UserName), Dto.Password);
      if (resu.Succeeded)
      {
        SubmitMessage[0] = "Account has has been created";
      }
      else
      {
        SubmitMessage = resu.Errors.Select(err => err.Description).ToList();
      }





    }




    // De rest van het programma hoeft de DTO niet te kennen.
    private class UserDto
    {

      public string UserName { get; set; } = string.Empty;
      public string Password { get; set; } = string.Empty;
      public string Email { get; set; } = string.Empty;

    }




  }









}