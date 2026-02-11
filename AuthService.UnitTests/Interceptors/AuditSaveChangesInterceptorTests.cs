using AuthService.Data;
using AuthService.Domain;
using AuthService.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AuthService.UnitTests.Interceptors;

public class AuditSaveChangesInterceptorTests : TestBase
{
    private static DataContext CreateDb(ICurrentUser currentUser)
    {
        var interceptor = new AuditSaveChangesInterceptor(currentUser);

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new DataContext(options);
    }

    private static Mock<ICurrentUser> CreateUser()
    {
        var mock = new Mock<ICurrentUser>();
        mock.Setup(x => x.UserName).Returns("tester");
        mock.Setup(x => x.LoginName).Returns((string?)null);
        mock.Setup(x => x.UserId).Returns(1);
        return mock;
    }

    // =========================
    // INSERT
    // =========================
    [Fact]
    public async Task Insert_Should_Set_CreatedDate()
    {
        var userMock = CreateUser();
        var db = CreateDb(userMock.Object);

        var user = new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.CreatedDate.Should().NotBe(default);
        user.CreatedBy.Should().Be("tester");
    }

    // =========================
    // UPDATE
    // =========================
    [Fact]
    public async Task Update_Should_Set_UpdatedDate()
    {
        var userMock = CreateUser();
        var db = CreateDb(userMock.Object);

        var user = new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.Username = "changed";
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.UpdatedDate.Should().NotBeNull();
        user.UpdatedBy.Should().Be("tester");
    }

    // =========================
    // SOFT DELETE
    // =========================
    [Fact]
    public async Task Delete_Should_Be_SoftDelete()
    {
        var userMock = CreateUser();
        var db = CreateDb(userMock.Object);

        var user = new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        db.AuthUser.Remove(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.IsDeleted.Should().BeTrue();
        user.DeletedBy.Should().Be("tester");
    }

    // =========================
    // SKIP AUDIT
    // =========================
    [Fact]
    public async Task SkipAudit_Should_Not_Set_UpdatedDate()
    {
        var userMock = CreateUser();
        var db = CreateDb(userMock.Object);

        var user = new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.Username = "changed";

        db.Flags.SkipAudit = true;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.UpdatedDate.Should().BeNull();
    }
}