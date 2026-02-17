using AuthService.Controllers;
using AuthService.Contracts.Request;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AuthService.Contracts.DTOs;
using AuthService.Common.Results;
using AuthService.Services.Users;

namespace AuthService.UnitTests.Controllers;

public class UserControllerTests : TestBase
{
    private readonly Mock<IUserService> _service = new();

    private UserController CreateController()
    {
        return new UserController(_service.Object);
    }

    // =========================
    // GET LIST SUCCESS
    // =========================
    [Fact]
    public async Task GetList_Should_Return_Ok()
    {
        var controller = CreateController();

        var data = new PagedResult<UserListItemDto>(
            Items: [],
            TotalCount: 0,
            Page: 1,
            PageSize: 10);

        _service.Setup(x => x.GetListAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<UserListItemDto>>.Success(data));

        var result = await controller.GetList(1, 10, TestContext.Current.CancellationToken);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
    }

    // =========================
    // GET LIST FAIL
    // =========================
    [Fact]
    public async Task GetList_Should_Return_BadRequest()
    {
        var controller = CreateController();

        _service.Setup(x => x.GetListAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<UserListItemDto>>.Failure("error"));

        var result = await controller.GetList(1, 10, TestContext.Current.CancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================
    // UPDATE SUCCESS
    // =========================
    [Fact]
    public async Task Update_Should_Return_Ok()
    {
        var controller = CreateController();

        var dto = new UserListItemDto(
            Id: 1,
            Username: "test",
            Email: "test@mail.com",
            IsActive: true,
            IsLocked: false,
            Roles: ["Admin"],
            CreatedDate: DateTime.UtcNow
        );

        _service.Setup(x => x.UpdateAsync(1, It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserListItemDto>.Success(dto));

        var result = await controller.Update(1, new UpdateUserRequest(), TestContext.Current.CancellationToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    // =========================
    // UPDATE NOT FOUND
    // =========================
    [Fact]
    public async Task Update_Should_Return_NotFound()
    {
        var controller = CreateController();

        _service.Setup(x => x.UpdateAsync(1, It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserListItemDto>.Failure("user not found"));

        var result = await controller.Update(1, new UpdateUserRequest(), TestContext.Current.CancellationToken);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========================
    // UPDATE BAD REQUEST
    // =========================
    [Fact]
    public async Task Update_Should_Return_BadRequest()
    {
        var controller = CreateController();

        _service.Setup(x => x.UpdateAsync(1, It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserListItemDto>.Failure("validation error"));

        var result = await controller.Update(1, new UpdateUserRequest(), TestContext.Current.CancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================
    // DELETE SUCCESS
    // =========================
    [Fact]
    public async Task Delete_Should_Return_Ok()
    {
        var controller = CreateController();

        _service.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await controller.SoftDelete(1, TestContext.Current.CancellationToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    // =========================
    // DELETE NOT FOUND
    // =========================
    [Fact]
    public async Task Delete_Should_Return_NotFound()
    {
        var controller = CreateController();

        _service.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure("not found"));

        var result = await controller.SoftDelete(1, TestContext.Current.CancellationToken);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========================
    // DELETE BAD REQUEST
    // =========================
    [Fact]
    public async Task Delete_Should_Return_BadRequest()
    {
        var controller = CreateController();

        _service.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure("db error"));

        var result = await controller.SoftDelete(1, TestContext.Current.CancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
