using AuthService.Domain;

namespace AuthService.Services
{
    public interface ITokenService
    {
        (string Token, int ExpiresInSeconds) GenerateAccessToken(AuthUser user, IEnumerable<string> roles);
    }

}
