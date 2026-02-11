using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Domain;
using AuthService.Repositories;
using AuthService.Services;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Services;

public class RoleServiceTests : TestBase
{
    private readonly Mock<IRoleRepository> _repo = new();

    private RoleService CreateService()
    {
        return new RoleService(_repo.Object);
    }

    private static AuthRole CreateRole()
    {
        return new AuthRole
        {
            AuthRoleId = 1,
            Name = "Admin",
            Description = "Admin role",
            CreatedDate = DateTime.UtcNow
        };
    }

    // =========================
    // CREATE SUCCESS
    // =========================
    [Fact]
    public async Task Create_Should_Return_Success()
    {
        var service = CreateService();
        var role = CreateRole();

        _repo.Setup(x => x.RoleNameAlreadyExists("Admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repo.Setup(x => x.CreateAsync("Admin", "desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await service.CreateAsync(
            new RoleRequest { Name = "Admin", Description = "desc" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Admin");
    }

    // =========================
    // CREATE DUPLICATE
    // =========================
    [Fact]
    public async Task Create_Should_Fail_When_Name_Exists()
    {
        var service = CreateService();

        _repo.Setup(x => x.RoleNameAlreadyExists("Admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // =========================
    // CREATE FAIL
    // =========================
    [Fact]
    public async Task Create_Should_Fail_When_Create_Returns_Null()
    {
        var service = CreateService();

        _repo.Setup(x => x.RoleNameAlreadyExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repo.Setup(x => x.CreateAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthRole?)null);

        var result = await service.CreateAsync(
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // =========================
    // GET LIST SUCCESS
    // =========================
    [Fact]
    public async Task GetList_Should_Return_Data()
    {
        var service = CreateService();

        var roles = new List<AuthRole>
        {
            CreateRole()
        };

        _repo.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((roles, 1));

        var result = await service.GetListAsync(
            1,
            10,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    // =========================
    // GET LIST NORMALIZE PAGE
    // =========================
    [Fact]
    public async Task GetList_Should_Normalize_Page()
    {
        var service = CreateService();

        _repo.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0));

        await service.GetListAsync(0, 0, TestContext.Current.CancellationToken);

        _repo.Verify(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================
    // UPDATE SUCCESS
    // =========================
    [Fact]
    public async Task Update_Should_Return_Dto()
    {
        var service = CreateService();
        var role = CreateRole();

        _repo.Setup(x => x.UpdateAsync(1, "Admin", "desc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await service.UpdateAsync(
            1,
            new RoleRequest { Name = "Admin", Description = "desc" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    // =========================
    // UPDATE NOT FOUND
    // =========================
    [Fact]
    public async Task Update_Should_Fail_When_NotFound()
    {
        var service = CreateService();

        _repo.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthRole?)null);

        var result = await service.UpdateAsync(
            1,
            new RoleRequest { Name = "Admin" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // =========================
    // DELETE SUCCESS
    // =========================
    [Fact]
    public async Task Delete_Should_Return_True()
    {
        var service = CreateService();

        _repo.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.SoftDeleteAsync(
            1,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    // =========================
    // DELETE FAIL
    // =========================
    [Fact]
    public async Task Delete_Should_Fail_When_NotFound()
    {
        var service = CreateService();

        _repo.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.SoftDeleteAsync(
            1,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }
}