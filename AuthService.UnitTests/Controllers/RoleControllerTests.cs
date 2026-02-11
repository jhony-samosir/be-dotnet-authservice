using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Controllers;
using AuthService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthService.UnitTests.Controllers;

public class RoleControllerTests : TestBase
{
    private readonly Mock<IRoleService> _service = new();

    private RoleController CreateController()
    {
        return new RoleController(_service.Object);
    }

    private static RoleListItemDto CreateDto()
    {
        return new RoleListItemDto(
                Id: 1,
                Name: "Admin",
                Description: "desc",
                CreatedDate: DateTime.UtcNow
            );
    }

    // =========================
    // CREATE SUCCESS
    // =========================
    [Fact]
    public async Task Create_Should_Return_201()
    {
        var controller = CreateController();

        _service.Setup(x => x.CreateAsync(It.IsAny<RoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RoleListItemDto>.Success(CreateDto()));

        var result = await controller.CreateRole(
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        var obj = result as ObjectResult;
        obj!.StatusCode.Should().Be(201);
    }

    // =========================
    // CREATE DUPLICATE
    // =========================
    [Fact]
    public async Task Create_Should_Return_Conflict()
    {
        var controller = CreateController();

        _service.Setup(x => x.CreateAsync(It.IsAny<RoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RoleListItemDto>.Failure("Role name already exists"));

        var result = await controller.CreateRole(
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    // =========================
    // CREATE FAIL
    // =========================
    [Fact]
    public async Task Create_Should_Return_BadRequest()
    {
        var controller = CreateController();

        _service.Setup(x => x.CreateAsync(It.IsAny<RoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RoleListItemDto>.Failure("error"));

        var result = await controller.CreateRole(
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================
    // GET LIST
    // =========================
    [Fact]
    public async Task GetList_Should_Return_OK()
    {
        var controller = CreateController();

        var paged = new PagedResult<RoleListItemDto>(
            [CreateDto()],
            1,
            1,
            10);

        _service.Setup(x => x.GetListAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<RoleListItemDto>>.Success(paged));

        var result = await controller.GetList(
            1,
            10,
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    // =========================
    // UPDATE SUCCESS
    // =========================
    [Fact]
    public async Task Update_Should_Return_OK()
    {
        var controller = CreateController();

        _service.Setup(x => x.UpdateAsync(1, It.IsAny<RoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RoleListItemDto>.Success(CreateDto()));

        var result = await controller.Update(
            1,
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    // =========================
    // UPDATE NOT FOUND
    // =========================
    [Fact]
    public async Task Update_Should_Return_404()
    {
        var controller = CreateController();

        _service.Setup(x => x.UpdateAsync(1, It.IsAny<RoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RoleListItemDto>.Failure("not found"));

        var result = await controller.Update(
            1,
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========================
    // DELETE SUCCESS
    // =========================
    [Fact]
    public async Task Delete_Should_Return_OK()
    {
        var controller = CreateController();

        _service.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await controller.SoftDelete(
            1,
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<OkObjectResult>();
    }

    // =========================
    // DELETE NOT FOUND
    // =========================
    [Fact]
    public async Task Delete_Should_Return_404()
    {
        var controller = CreateController();

        _service.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure("not found"));

        var result = await controller.SoftDelete(
            1,
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}