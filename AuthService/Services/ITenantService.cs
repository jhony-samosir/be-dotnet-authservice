using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;

namespace AuthService.Services;

/// <summary>
/// Application service for tenant list, update, and soft delete.
/// </summary>
public interface ITenantService
{
    Task<Result<TenantListProjection>> CreateAsync(string name, CancellationToken ct = default);
    Task<Result<TenantListProjection>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<PagedResult<TenantListProjection>>> GetListAsync(QueryOptions query, CancellationToken ct = default);
    Task<Result<bool>> SoftDeleteAsync(int id, CancellationToken ct = default);
    Task<Result<TenantListProjection>> UpdateAsync(int id, string name, CancellationToken ct = default);
}