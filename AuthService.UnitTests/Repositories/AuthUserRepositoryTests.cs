using AuthService.Data;
using AuthService.Domain;
using AuthService.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.UnitTests.Repositories;

public class AuthUserRepositoryTests : TestBase
{
    // =========================
    // EXISTS EMAIL
    // =========================
    [Fact]
    public async Task ExistsByEmail_Should_Return_True()
    {
        var db = CreateDb();

        db.AuthUser.Add(new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        });

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new AuthUserRepository(db);

        var result = await repo.ExistsByEmailAsync("test", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    // =========================
    // CREATE USER
    // =========================
    [Fact]
    public async Task Create_Should_Create_User_And_Role()
    {
        var db = CreateDb();

        db.AuthRole.Add(new AuthRole
        {
            Name = "Default",
            Description = "Default"
        });

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new AuthUserRepository(db);

        var (user, roles) = await repo.CreateAsync("test@mail.com", "hash", TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        roles.Should().Contain("Default");
    }

    // =========================
    // GET BY EMAIL
    // =========================
    [Fact]
    public async Task GetByEmail_Should_Return_User()
    {
        var db = CreateDb();

        var role = new AuthRole { Name = "Admin" };
        db.AuthRole.Add(role);

        var user = new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        db.AuthUserRole.Add(new AuthUserRole
        {
            AuthUserId = user.AuthUserId,
            AuthRoleId = role.AuthRoleId
        });

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new AuthUserRepository(db);

        var (u, roles) = await repo.GetByEmailWithRolesAsync("test", TestContext.Current.CancellationToken);

        u.Should().NotBeNull();
        roles.Should().Contain("Admin");
    }

    // =========================
    // PAGED
    // =========================
    [Fact]
    public async Task GetPaged_Should_Return_Items()
    {
        var db = CreateDb();

        for (int i = 0; i < 5; i++)
        {
            db.AuthUser.Add(new AuthUser
            {
                Username = "u" + i,
                Email = $"u{i}@mail.com",
                PasswordHash = "x",
                AuthTenantId = 1
            });
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new AuthUserRepository(db);

        var (items, total) = await repo.GetPagedAsync(1, 10, TestContext.Current.CancellationToken);

        total.Should().Be(5);
        items.Should().HaveCount(5);
    }

    // =========================
    // UPDATE
    // =========================
    [Fact]
    public async Task Update_Should_Update_Email()
    {
        var db = CreateDb();

        var user = new AuthUser
        {
            Username = "test",
            Email = "old@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new AuthUserRepository(db);

        var (updated, _) = await repo.UpdateAsync(user.AuthUserId, "new@mail.com", true, false, null, TestContext.Current.CancellationToken);

        updated!.Email.Should().Be("new@mail.com");
    }

    // =========================
    // SOFT DELETE
    // =========================
    [Fact]
    public async Task SoftDelete_Should_Delete_User()
    {
        var db = CreateDb();

        var user = new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new AuthUserRepository(db);

        var deleted = await repo.SoftDeleteAsync(user.AuthUserId, TestContext.Current.CancellationToken);

        deleted.Should().BeTrue();
    }

    // =========================
    // AUDIT LOGIN
    // =========================
    [Fact]
    public async Task AuditLogin_Should_Update_LastLogin()
    {
        var db = CreateDb();

        var user = new AuthUser
        {
            Username = "test",
            Email = "test@mail.com",
            PasswordHash = "x",
            AuthTenantId = 1
        };

        db.AuthUser.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new AuthUserRepository(db);

        var result = await repo.AuditLoginDate(user.AuthUserId, TestContext.Current.CancellationToken);

        result.Should().BeTrue();

        var updated = await db.AuthUser.FirstAsync(TestContext.Current.CancellationToken);
        updated.LastLoginDate.Should().NotBeNull();
    }
}