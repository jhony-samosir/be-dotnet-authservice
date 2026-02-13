namespace AuthService.Services;

public interface ITokenService
{
    (string Token, int ExpiresInSeconds) GenerateAccessToken(
        int userId,
        string username,
        IEnumerable<string> roles);
}