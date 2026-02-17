using AuthService.Configuration;
using AuthService.Domain;
using AuthService.Services.Tokens;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.UnitTests.Services;

public class TokenServiceTests : TestBase
{
    private static TokenService CreateService()
    {
        var settings = new JwtSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningKey = "THIS_IS_A_TEST_SIGNING_KEY_123456789",
            AccessTokenSeconds = 600
        };

        var options = Options.Create(settings);
        return new TokenService(options);
    }

    private static AuthUser CreateUser()
    {
        return new AuthUser
        {
            AuthUserId = 10,
            Username = "john",
            Email = "john@mail.com"
        };
    }

    // =========================
    // TOKEN GENERATE
    // =========================
    [Fact]
    public void GenerateToken_Should_Return_Token()
    {
        var service = CreateService();
        var user = CreateUser();

        var (token, expires) = service.GenerateAccessToken(user.AuthUserId, user.Username, ["Admin"]);

        token.Should().NotBeNullOrEmpty();
        expires.Should().BeGreaterThan(0);
    }

    // =========================
    // CLAIM CHECK
    // =========================
    [Fact]
    public void Token_Should_Contain_User_Claims()
    {
        var service = CreateService();
        var user = CreateUser();

        var (token, _) = service.GenerateAccessToken(user.AuthUserId, user.Username, ["Admin"]);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(x => x.Type == ClaimTypes.Name && x.Value == "john");
        jwt.Claims.Should().Contain(x => x.Type == ClaimTypes.NameIdentifier && x.Value == "10");
        jwt.Claims.Should().Contain(x => x.Type == ClaimTypes.Role && x.Value == "Admin");
    }

    // =========================
    // ISSUER AUDIENCE
    // =========================
    [Fact]
    public void Token_Should_Have_Issuer_And_Audience()
    {
        var service = CreateService();
        var user = CreateUser();

        var (token, _) = service.GenerateAccessToken(user.AuthUserId, user.Username, []);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
    }

    // =========================
    // EXPIRY CHECK
    // =========================
    [Fact]
    public void Token_Should_Have_Expiry()
    {
        var service = CreateService();
        var user = CreateUser();

        var (_, expires) = service.GenerateAccessToken(user.AuthUserId ,user.Username, []);

        expires.Should().BeGreaterThan(0);
        expires.Should().BeLessThanOrEqualTo(3600);
    }
}