using Microsoft.AspNetCore.Identity;
using USPSimGame.Data.Entities;

namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class CreateUser
  {

    private ApplicationUser? User { get; set; } = new();
    private bool IsSend { get; set; }

    private void AddUser()
    {


    }

    public class ChipDog
    {
      public string? Id { get; set; }
    }
  }
}