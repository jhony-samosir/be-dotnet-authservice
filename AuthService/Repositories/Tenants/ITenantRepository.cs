using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;

namespace AuthService.Repositories.Tenants;

/// <summary>
/// Repository abstraction for querying tenant.
/// Keeps EF Core details out of the application service.
/// </summary>
public interface ITenantRepository
{
    Task<PagedResult<TenantListProjection>> GetPagedAsync(QueryOptions query, CancellationToken ct = default);
    Task<TenantListProjection> CreateAsync(string name, CancellationToken ct = default);
    Task<bool> ExistByNameAsync(string name, CancellationToken ct = default);
    Task<TenantListProjection> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TenantListProjection> UpdateAsync(int id, string name, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
}
