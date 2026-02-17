using Microsoft.AspNetCore.Identity;

namespace AuthService.Services.Passwords;

/// <summary>
/// Implements password hash & verify. <see cref="IPasswordService"/>.
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null!, password);

    public bool Verify(string hash, string password)
    {
        var result = _hasher.VerifyHashedPassword(null!, hash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
