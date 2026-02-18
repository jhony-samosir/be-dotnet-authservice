using AuthService.Configuration;
using AuthService.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services.Tokens;

public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public (string Token, int ExpiresInSeconds) GenerateAccessToken(
        int userId,
        string email,
        IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.SigningKey));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var username = email.Contains('@') 
            ? email.Split('@')[0] : email;

        var now = DateTime.UtcNow;
        var expires = now.AddSeconds(_jwt.AccessTokenSeconds);

        var claims = new List<Claim>
        {
            // STANDARD JWT CLAIMS
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Email, username),

            // ENTERPRISE CUSTOM
            new(AppConstants.UserId, userId.ToString()),
            new(AppConstants.UserName, username),
            new(AppConstants.Email, username)
        };

        // roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim(AppConstants.RoleId, role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresInSeconds = (int)(expires - now).TotalSeconds;

        return (tokenString, expiresInSeconds);
    }


    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
