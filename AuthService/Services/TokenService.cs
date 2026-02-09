using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Configuration;
using AuthService.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    /// <summary>
    /// Responsible for generating JWT access tokens.
    /// Depends only on configuration and BCL types, which keeps it
    /// easy to test and reuse.
    /// Implement of <see cref="ITokenService"/>.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public (string Token, int ExpiresInSeconds) GenerateAccessToken(AuthUser user, IEnumerable<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_jwtSettings.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.AuthUserId.ToString()),
                new(ClaimTypes.NameIdentifier, user.AuthUserId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Username),
                new(ClaimTypes.Name, user.Username),

                new("CurrentUserID", user.AuthUserId.ToString()),
                new("CurrentUserName", user.Username),
                new("LoginName", user.Username)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var expiresInSeconds = (int)(expires - now).TotalSeconds;

            return (tokenString, expiresInSeconds);
        }
    }
}

