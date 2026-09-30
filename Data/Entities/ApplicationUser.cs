


using Microsoft.AspNetCore.Identity;

namespace USPSimGame.Data.Entities;

// https://www.reddit.com/r/dotnet/comments/14l8b7m/should_i_remove_unnecessary_tables_from_identity/

/// <summary>
/// The class <c>ApplicationUser</c> inherits from a default IdentityUser for ASP.NET Identity framework. <br/>
/// We will be using this framework for dealing with options like password length or allowed charachter. <br/>
/// This inherited class creates 7 tables for the full framework and it's considered ok to leave some empty
///</summary>
public class ApplicationUser : IdentityUser
{

}