using AuthService.Configuration;
using Microsoft.Extensions.Options;

namespace AuthService.Services.Cookies;

public class CookieService(
    IHttpContextAccessor httpContextAccessor, 
    IOptions<JwtSettings> jwtOptions) : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;
    private const string RefreshTokenCookieName = "refreshToken";

    public void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to false for HTTP localhost development. Set to true in Production!
            SameSite = SameSiteMode.Lax, 
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTTLDays),
            Path = "/", 
            IsEssential = true
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            RefreshTokenCookieName, 
            token, 
            cookieOptions);
    }

    public string? GetRefreshTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];
    }

    public void DeleteRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Match the Set method
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };
            
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);
    }
}
