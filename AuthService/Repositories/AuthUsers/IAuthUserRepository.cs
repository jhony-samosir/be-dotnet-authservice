using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;

namespace AuthService.Repositories.AuthUsers;

/// <summary>
/// Repository abstraction for querying authentication users and their roles.
/// Keeps EF Core details out of the application service.
/// </summary>

public interface IAuthUserRepository
{
    Task<UserWithRolesProjection?> GetByEmailWithRolesAsync(
        string emailOrUsername,
        CancellationToken cancellationToken = default);

    Task<string?> GetPasswordHashAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> AuditLoginDateAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string emailOrUsername,
        CancellationToken cancellationToken = default);

    Task<UserListProjection> CreateAsync(
        string emailOrUsername,
        string passwordHash,
        CancellationToken cancellationToken = default);

    Task<UserListProjection?> UpdateAsync(
        int userId,
        string? email,
        bool? isActive,
        bool? isLocked,
        IReadOnlyList<string>? roleNames,
        CancellationToken cancellationToken = default);

    Task<UserWithRolesProjection?> GetByIdWithRolesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<UserListProjection>> GetPagedAsync(
        QueryOptions query,
        CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
