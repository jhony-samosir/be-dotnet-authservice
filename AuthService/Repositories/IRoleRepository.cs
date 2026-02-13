using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;

namespace AuthService.Repositories;

/// <summary>
/// Repository abstraction for querying roles.
/// Keeps EF Core details out of the application service.
/// </summary>
public interface IRoleRepository
{
    Task<RoleListProjection> CreateAsync(
    string name,
    string? description,
    CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    Task<RoleListProjection?> GetByIdAsync(
        int roleId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RoleListProjection>> GetPagedAsync(
        QueryOptions query,
        CancellationToken cancellationToken = default);

    Task<RoleListProjection?> UpdateAsync(
        int roleId,
        string name,
        string? description,
        CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(
        int roleId,
        CancellationToken cancellationToken = default);
}
