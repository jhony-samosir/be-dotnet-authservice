using AuthService.Common;
using AuthService.Common.Results;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Controllers;
using AuthService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthService.UnitTests.Controllers;

public class AuthControllerTests : TestBase
{
    private readonly Mock<IAuthService> _service = new();

    private AuthController CreateController()
    {
        return new AuthController(_service.Object);
    }

    private static AuthResponse CreateResponse()
    {
        return new AuthResponse(
                AccessToken: "TOKEN",
                ExpiresIn: 3600,
                TokenType: "Bearer",
                User: new UserInfo(1, "test", ["Admin"])
            );
    }

    // =========================
    // LOGIN SUCCESS
    // =========================
    [Fact]
    public async Task Login_Should_Return_OK()
    {
        var controller = CreateController();

        _service.Setup(x => x.Login(It.IsAny<AuthRequest>(), default))
            .ReturnsAsync(Result<AuthResponse>.Success(CreateResponse()));

        var result = await controller.Login(new AuthRequest());

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();

        var response = ok!.Value as ApiResponse<AuthResponse>;
        response!.Success.Should().BeTrue();
    }

    // =========================
    // LOGIN FAIL
    // =========================
    [Fact]
    public async Task Login_Should_Return_Unauthorized()
    {
        var controller = CreateController();

        _service.Setup(x => x.Login(It.IsAny<AuthRequest>(), default))
            .ReturnsAsync(Result<AuthResponse>.Failure("Invalid"));

        var result = await controller.Login(new AuthRequest());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =========================
    // REGISTER SUCCESS
    // =========================
    [Fact]
    public async Task Register_Should_Return_201()
    {
        var controller = CreateController();

        _service.Setup(x => x.Register(It.IsAny<RegisterRequest>(), default))
            .ReturnsAsync(Result<AuthResponse>.Success(CreateResponse()));

        var result = await controller.Register(new RegisterRequest());

        var created = result as ObjectResult;
        created!.StatusCode.Should().Be(201);
    }

    // =========================
    // REGISTER FAIL
    // =========================
    [Fact]
    public async Task Register_Should_Return_Conflict()
    {
        var controller = CreateController();

        _service.Setup(x => x.Register(It.IsAny<RegisterRequest>(), default))
            .ReturnsAsync(Result<AuthResponse>.Failure("exists"));

        var result = await controller.Register(new RegisterRequest());

        result.Should().BeOfType<ConflictObjectResult>();
    }
}