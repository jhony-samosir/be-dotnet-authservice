using AuthService.Common.Results;

namespace AuthService.Services.Cookies;

public interface ICookieService
{
    void SetRefreshTokenCookie(string token);
    string? GetRefreshTokenFromCookie();
    void DeleteRefreshTokenCookie();
}
