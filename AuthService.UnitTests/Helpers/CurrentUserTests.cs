using AuthService.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace AuthService.UnitTests.Helpers;

public class CurrentUserTests : TestBase
{
    private static CurrentUser CreateUser(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(httpContext);

        return new CurrentUser(accessor.Object);
    }

    // =========================
    // AUTHENTICATED
    // =========================
    [Fact]
    public void Should_Be_Authenticated()
    {
        var user = CreateUser(new Claim("sub", "1"));

        user.IsAuthenticated.Should().BeTrue();
    }

    // =========================
    // USER ID
    // =========================
    [Fact]
    public void Should_Read_UserId()
    {
        var user = CreateUser(
            new Claim(AppConstants.UserId, "10")
        );

        user.UserId.Should().Be(10);
    }

    // =========================
    // USERNAME
    // =========================
    [Fact]
    public void Should_Read_UserName()
    {
        var user = CreateUser(
            new Claim(AppConstants.UserName, "john")
        );

        user.UserName.Should().Be("john");
    }

    // =========================
    // EMAIL
    // =========================
    [Fact]
    public void Should_Read_Email()
    {
        var user = CreateUser(
            new Claim(AppConstants.Email, "test@mail.com")
        );

        user.Email.Should().Be("test@mail.com");
    }

    // =========================
    // LOGIN NAME
    // =========================
    [Fact]
    public void Should_Read_LoginName()
    {
        var user = CreateUser(
            new Claim(AppConstants.LoginName, "login")
        );

        user.LoginName.Should().Be("login");
    }

    // =========================
    // ADMIN FLAG
    // =========================
    [Fact]
    public void Should_Read_IsAdmin()
    {
        var user = CreateUser(
            new Claim(AppConstants.IsAdmin, "true")
        );

        user.IsAdmin.Should().BeTrue();
    }

    // =========================
    // TOKEN
    // =========================
    [Fact]
    public void Should_Read_Token_From_Header()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer ABC123";

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(httpContext);

        var current = new CurrentUser(accessor.Object);

        current.Token.Should().Be("ABC123");
    }

    // =========================
    // NO CLAIM
    // =========================
    [Fact]
    public void Should_Return_Default_When_No_Claim()
    {
        var user = CreateUser();

        user.UserId.Should().Be(0);
        user.UserName.Should().BeNull();
        user.Email.Should().BeNull();
    }
}