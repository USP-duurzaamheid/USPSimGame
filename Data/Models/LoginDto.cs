
using System.ComponentModel.DataAnnotations;

namespace USPSimGame.Data.Models.LoginDto;

/// <summary>
/// This data transfer object is being used to bridge the blazor <c>UserDto</c> inside <c>CreateUser.razor.cs</c> with the /login endpoint.
/// </summary>
public class LoginDto
{
  [Required(ErrorMessage = "Enter a valid UserName")]
  public string UserName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Password is Required")]
  public string Password { get; set; } = string.Empty;
}