using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Domain;
using AuthService.Repositories.AuthUsers;
using AuthService.Services.Users;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Services;

public class UserServiceTests : TestBase
{
    private readonly Mock<IAuthUserRepository> _repo = new();

    private UserService CreateService()
    {
        return new UserService(_repo.Object);
    }

    // =========================
    // GET LIST SUCCESS
    // =========================
    [Fact]
    public async Task GetList_Should_Return_PagedResult()
    {
        var service = CreateService();

        var user = new AuthUser
        {
            AuthUserId = 1,
            Username = "test",
            Email = "test@mail.com",
            IsActive = true,
            IsLocked = false,
            CreatedDate = DateTime.UtcNow
        };

        var data = new List<(AuthUser User, List<string> Roles)>
        {
            (user, new List<string>{ "Admin" })
        };

        (List<(AuthUser User, List<string> Roles)> Items, int TotalCount) tuple
            = (data, 1);

        _repo.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tuple);

        var result = await service.GetListAsync(1, 10, Ct);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    // =========================
    // PAGE NORMALIZE
    // =========================
    [Fact]
    public async Task GetList_Should_Normalize_Page()
    {
        var service = CreateService();

        var empty = new List<(AuthUser User, List<string> Roles)>();

        _repo.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((empty, 0));

        await service.GetListAsync(0, 0, Ct);

        _repo.Verify(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================
    // UPDATE SUCCESS
    // =========================
    [Fact]
    public async Task Update_Should_Return_Dto()
    {
        var service = CreateService();

        var user = new AuthUser
        {
            AuthUserId = 1,
            Username = "test",
            Email = "new@mail.com",
            IsActive = true,
            IsLocked = false,
            CreatedDate = DateTime.UtcNow
        };

        _repo.Setup(x => x.UpdateAsync(
                1,
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, new List<string> { "Admin" }));

        var result = await service.UpdateAsync(
            1,
            new UpdateUserRequest
            {
                Email = "new@mail.com",
                IsActive = true,
                IsLocked = false,
                Roles = ["Admin"]
            },
            Ct);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("new@mail.com");
    }

    // =========================
    // UPDATE NOT FOUND
    // =========================
    [Fact]
    public async Task Update_Should_Return_Failure_When_User_Not_Found()
    {
        var service = CreateService();

        _repo.Setup(x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<IReadOnlyList<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(((AuthUser?)null, []));

        var result = await service.UpdateAsync(1, new UpdateUserRequest(), Ct);

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

        var result = await service.SoftDeleteAsync(1, Ct);

        result.IsSuccess.Should().BeTrue();
    }

    // =========================
    // DELETE FAIL
    // =========================
    [Fact]
    public async Task Delete_Should_Return_Failure()
    {
        var service = CreateService();

        _repo.Setup(x => x.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.SoftDeleteAsync(1, Ct);

        result.IsSuccess.Should().BeFalse();
    }
}