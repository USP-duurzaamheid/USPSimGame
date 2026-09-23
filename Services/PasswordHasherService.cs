using Microsoft.AspNetCore.Identity;

namespace USPSimGame.Services;

public class PasswordHasherService : IPasswordHasher
{
    private class DummyUser { }
    private readonly PasswordHasher<DummyUser> _hasher = new();
    private readonly DummyUser _user = new();

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return string.Empty;
        }

        return _hasher.HashPassword(_user, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) && string.IsNullOrEmpty(providedPassword))
        {
            return true;
        }

        if (string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(_user, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
