using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Domain;
using AuthService.Repositories;
using AuthService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AuthService.UnitTests.Services;

public class AuthServiceTests : TestBase
{
    private readonly Mock<IAuthUserRepository> _repo = new();
    private readonly Mock<ITokenService> _token = new();
    private readonly Mock<IPasswordHasher<AuthUser>> _hasher = new();

    private AuthService.Services.AuthService CreateService()
    {
        AuthService.Services.AuthService authService = new(_repo.Object, _token.Object, _hasher.Object);
        return authService;
    }

    private static AuthUser CreateUser()
        => new()
        {
            AuthUserId = 1,
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "HASH",
            IsActive = true,
            IsDeleted = false,
            IsLocked = false
        };

    // =========================
    // LOGIN SUCCESS
    // =========================
    [Fact]
    public async Task Login_Should_Return_Token()
    {
        var service = CreateService();
        var user = CreateUser();
        var roles = new List<string> { "Admin" };

        _repo.Setup(x => x.GetByEmailWithRolesAsync("test@mail.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, roles));

        _hasher.Setup(x => x.VerifyHashedPassword(user, "HASH", "123"))
            .Returns(PasswordVerificationResult.Success);

        _repo.Setup(x => x.AuditLoginDate(user.AuthUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _token.Setup(x => x.GenerateAccessToken(user, roles))
            .Returns(("TOKEN", 3600));

        var result = await service.Login(
            new AuthRequest { Email = "test@mail.com", Password = "123" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("TOKEN");
    }

    // =========================
    // LOGIN USER NOT FOUND
    // =========================
    [Fact]
    public async Task Login_Should_Fail_When_User_Not_Found()
    {
        var service = CreateService();

        _repo.Setup(x => x.GetByEmailWithRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((AuthUser?)null, []));

        var result = await service.Login(
            new AuthRequest { Email = "x", Password = "x" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // =========================
    // LOGIN WRONG PASSWORD
    // =========================
    [Fact]
    public async Task Login_Should_Fail_When_Password_Wrong()
    {
        var service = CreateService();
        var user = CreateUser();

        _repo.Setup(x => x.GetByEmailWithRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, new List<string>()));

        _hasher.Setup(x => x.VerifyHashedPassword(user, "HASH", "wrong"))
            .Returns(PasswordVerificationResult.Failed);

        var result = await service.Login(
            new AuthRequest { Email = "test", Password = "wrong" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // =========================
    // LOGIN AUDIT FAIL
    // =========================
    [Fact]
    public async Task Login_Should_Fail_When_Audit_Fails()
    {
        var service = CreateService();
        var user = CreateUser();

        _repo.Setup(x => x.GetByEmailWithRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, new List<string>()));

        _hasher.Setup(x => x.VerifyHashedPassword(user, "HASH", "123"))
            .Returns(PasswordVerificationResult.Success);

        _repo.Setup(x => x.AuditLoginDate(user.AuthUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.Login(
            new AuthRequest { Email = "test", Password = "123" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // =========================
    // REGISTER SUCCESS
    // =========================
    [Fact]
    public async Task Register_Should_Create_User()
    {
        var service = CreateService();

        _repo.Setup(x => x.ExistsByEmailAsync("test@mail.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hasher.Setup(x => x.HashPassword(It.IsAny<AuthUser>(), "123"))
            .Returns("HASH");

        var user = CreateUser();

        _repo.Setup(x => x.CreateAsync("test@mail.com", "HASH", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, new List<string> { "Default" }));

        _token.Setup(x => x.GenerateAccessToken(user, It.IsAny<List<string>>()))
            .Returns(("TOKEN", 3600));

        var result = await service.Register(
            new RegisterRequest { Email = "test@mail.com", Password = "123" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    // =========================
    // REGISTER EMAIL EXISTS
    // =========================
    [Fact]
    public async Task Register_Should_Fail_When_Email_Exists()
    {
        var service = CreateService();

        _repo.Setup(x => x.ExistsByEmailAsync("test@mail.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.Register(
            new RegisterRequest { Email = "test@mail.com", Password = "123" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }
}