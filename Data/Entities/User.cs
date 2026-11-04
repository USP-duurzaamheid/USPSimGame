namespace USPSimGame.Data.Entities;


/// <summary>
/// Class <c>User</c> will become deprecated! DO NOT USE IN FUTURE CODE! <br/>
/// This is due to the fact that the user class is not suitable when working with ASP.NET identity as a login manager
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}


