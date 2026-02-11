using AuthService.Data;
using AuthService.Domain;
using AuthService.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.UnitTests.Repositories;

public class RoleRepositoryTests : TestBase
{
    // =========================
    // CREATE
    // =========================
    [Fact]
    public async Task Create_Should_Insert_Role()
    {
        var db = CreateDb();
        var repo = new RoleRepository(db);

        var role = await repo.CreateAsync("Admin", "desc", TestContext.Current.CancellationToken);

        role.Should().NotBeNull();
        role!.Name.Should().Be("Admin");

        db.AuthRole.Count().Should().Be(1);
    }

    // =========================
    // EXISTS
    // =========================
    [Fact]
    public async Task Exists_Should_Return_True()
    {
        var db = CreateDb();

        db.AuthRole.Add(new AuthRole
        {
            Name = "Admin"
        });

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new RoleRepository(db);

        var exists = await repo.RoleNameAlreadyExists("Admin", TestContext.Current.CancellationToken);

        exists.Should().BeTrue();
    }

    // =========================
    // GET BY ID
    // =========================
    [Fact]
    public async Task GetById_Should_Return_Role()
    {
        var db = CreateDb();

        var role = new AuthRole { Name = "Admin" };
        db.AuthRole.Add(role);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new RoleRepository(db);

        var result = await repo.GetByIdAsync(role.AuthRoleId, false, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
    }

    // =========================
    // GET PAGED
    // =========================
    [Fact]
    public async Task GetPaged_Should_Return_Data()
    {
        var db = CreateDb();

        for (int i = 0; i < 5; i++)
        {
            db.AuthRole.Add(new AuthRole { Name = "R" + i });
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new RoleRepository(db);

        var (items, total) = await repo.GetPagedAsync(1, 10, TestContext.Current.CancellationToken);

        total.Should().Be(5);
        items.Should().HaveCount(5);
    }

    // =========================
    // UPDATE
    // =========================
    [Fact]
    public async Task Update_Should_Modify_Role()
    {
        var db = CreateDb();

        var role = new AuthRole { Name = "Old" };
        db.AuthRole.Add(role);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new RoleRepository(db);

        var updated = await repo.UpdateAsync(role.AuthRoleId, "New", "desc", TestContext.Current.CancellationToken);

        updated!.Name.Should().Be("New");
    }

    // =========================
    // UPDATE NOT FOUND
    // =========================
    [Fact]
    public async Task Update_Should_Return_Null_When_NotFound()
    {
        var db = CreateDb();
        var repo = new RoleRepository(db);

        var updated = await repo.UpdateAsync(999, "x", "x", TestContext.Current.CancellationToken);

        updated.Should().BeNull();
    }

    // =========================
    // SOFT DELETE
    // =========================
    [Fact]
    public async Task SoftDelete_Should_Delete()
    {
        var db = CreateDb();

        var role = new AuthRole { Name = "Admin" };
        db.AuthRole.Add(role);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repo = new RoleRepository(db);

        var deleted = await repo.SoftDeleteAsync(role.AuthRoleId, TestContext.Current.CancellationToken);

        deleted.Should().BeTrue();
    }

    // =========================
    // DELETE NOT FOUND
    // =========================
    [Fact]
    public async Task SoftDelete_Should_Return_False_When_NotFound()
    {
        var db = CreateDb();
        var repo = new RoleRepository(db);

        var deleted = await repo.SoftDeleteAsync(999, TestContext.Current.CancellationToken);

        deleted.Should().BeFalse();
    }
}