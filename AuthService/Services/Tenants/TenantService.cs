using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;
using AuthService.Repositories.Tenants;

namespace AuthService.Services.Tenants;

public class TenantService(ITenantRepository tenantRepository) : ITenantService
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task<Result<TenantListProjection>> CreateAsync(string name, CancellationToken ct = default)
    {
        var exist = await _tenantRepository.ExistByNameAsync(name, ct);
        if (exist)
            return Result<TenantListProjection>.Failure(new Error(ErrorCode.Conflict, "Tenant name already exists"));

        var tenant = await _tenantRepository.CreateAsync(name, ct);
        if (tenant == null)
            return Result<TenantListProjection>.Failure(new Error(ErrorCode.Conflict, "Tenant code already exists. Please try again!"));

        return Result<TenantListProjection>.Success(tenant);
    }

    public async Task<Result<TenantListProjection>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct);
        if (tenant == null)
            return Result<TenantListProjection>.Failure(new Error(ErrorCode.NotFound, "Tenant not found"));

        return Result<TenantListProjection>.Success(tenant);
    }

    public async Task<Result<PagedResult<TenantListProjection>>> GetListAsync(QueryOptions query, CancellationToken ct = default)
    {
        var tenants = await _tenantRepository.GetPagedAsync(query, ct);
        return Result<PagedResult<TenantListProjection>>.Success(tenants);
    }

    public async Task<Result<bool>> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var result = await _tenantRepository.SoftDeleteAsync(id, ct);
        if (!result)
            return Result<bool>.Failure(new Error(ErrorCode.NotFound, "Tenant not found"));

        return Result<bool>.Success(result);
    }

    public async Task<Result<TenantListProjection>> UpdateAsync(int id, string name, CancellationToken ct = default)
    {
        var exist = await _tenantRepository.ExistByNameAsync(name, ct);
        if (exist)
            return Result<TenantListProjection>.Failure(new Error(ErrorCode.Conflict, "Tenant name already exists"));

        var tenant = await _tenantRepository.UpdateAsync(id, name, ct);
        if (tenant == null)
            return Result<TenantListProjection>.Failure(new Error(ErrorCode.NotFound, "Tenant not found"));

        return Result<TenantListProjection>.Success(tenant);
    }
}