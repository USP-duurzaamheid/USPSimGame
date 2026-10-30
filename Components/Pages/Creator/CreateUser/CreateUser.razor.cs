using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Models.LoginDto;



namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class CreateUser(UserManager<ApplicationUser> userManager, NavigationManager navigationManager, RoleManager<IdentityRole> roleManager)
  {

    [SupplyParameterFromForm(FormName = "CreateUser")]
    private UserDto? Dto { get; set; }
    private List<string> SubmitMessage { get; set; } = new();
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private readonly RoleManager<IdentityRole> _roleUser = roleManager;

    private bool LoginBool { get; set; } = false;
    private readonly NavigationManager _navigationManager = navigationManager;

    protected override void OnInitialized()
    {
      Dto ??= new();

    }




    private async Task AddUser()
    {



      if (string.IsNullOrEmpty(Dto!.Email) || string.IsNullOrEmpty(Dto.Password) || string.IsNullOrEmpty(Dto.UserName))
      {
        SubmitMessage.Add("Please enter something in the fields");
        return;
      }


      var user = new ApplicationUser(Dto.Email, Dto.UserName);
      var resu = await _userManager.CreateAsync(user, Dto.Password);
      if (resu.Succeeded)
      {
        SubmitMessage.Add("Account has has been created");
        await _userManager.AddToRoleAsync(user, "Creator");

        LoginBool = true; // Update UI 

        await Task.Delay(2000);
        _navigationManager.NavigateTo("/login-user");
      }
      else
      {
        SubmitMessage = resu.Errors.Select(err => err.Description).ToList();
      }

    }

    // De rest van het programma hoeft de DTO niet te kennen.
    private class UserDto
    {

      [Required]
      public string UserName { get; set; } = string.Empty;

      [Required]
      public string Password { get; set; } = string.Empty;

      [Required]
      [EmailAddress]
      public string Email { get; set; } = string.Empty;

    }




  }









}