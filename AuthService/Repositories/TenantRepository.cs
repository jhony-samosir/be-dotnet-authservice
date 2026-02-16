using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthService.Repositories;

public class TenantRepository(DataContext dataContext) : ITenantRepository
{
    private readonly DataContext _dbContext = dataContext;

    public async Task<TenantListProjection> CreateAsync(string name, CancellationToken ct = default)
    {
        var code = await GenerateTenantCodeAsync();

        if (code == null)
        {
            return new TenantListProjection();
        }

        var tenant = new AuthTenant
        {
            Name = name,
            Code = code
        };

        _dbContext.AuthTenant.Add(tenant);
        await _dbContext.SaveChangesAsync(ct);

        return await GetProjectionById(tenant.AuthTenantId, ct);
    }

    public async Task<bool> ExistByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbContext.AuthTenant
            .AsNoTracking()
            .AnyAsync(x => x.Name == name && !x.IsDeleted, ct);
    }

    public async Task<TenantListProjection> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await GetProjectionById(id, ct);
    }

    public async Task<PagedResult<TenantListProjection>> GetPagedAsync(QueryOptions query, CancellationToken ct = default)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize <= 0 || query.PageSize > 100)
            query.PageSize = 10;

        var whitelist = new HashSet<string>
        {
            nameof(TenantListProjection.Name),
            nameof(TenantListProjection.Code),
            nameof(TenantListProjection.CreatedDate)
        };

        IQueryable<TenantListProjection> q =
            from t in _dbContext.AuthTenant.AsNoTracking()
            where !t.IsDeleted
            select new TenantListProjection
            {
                Id = t.AuthTenantId,
                Name = t.Name,
                Code = t.Code,
                CreatedDate = t.CreatedDate
            };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = $"%{query.Search}%";
            q = q.Where(x => EF.Functions.ILike(x.Name, s));
        }

        q = q.ApplyQuery(query, whitelist);

        var total = await q.CountAsync(ct);
        var items = await q.ApplyPaging(query).ToListAsync(ct);

        return new PagedResult<TenantListProjection>(
            items,
            total,
            query.Page,
            query.PageSize);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var tenant = await _dbContext.AuthTenant
            .FirstOrDefaultAsync(t => t.AuthTenantId == id && !t.IsDeleted, ct);

        if (tenant == null)
            return false;

        tenant.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<TenantListProjection> UpdateAsync(int id, string name, CancellationToken ct = default)
    {
        var tenant = await _dbContext.AuthTenant
            .FirstOrDefaultAsync(t => t.AuthTenantId == id && !t.IsDeleted, ct);

        if (tenant == null)
            return new TenantListProjection();

        tenant.Name = name;
        await _dbContext.SaveChangesAsync(ct);

        return await GetProjectionById(id, ct);
    }

    private async Task<TenantListProjection> GetProjectionById(int tenantId, CancellationToken ct)
    {
        return await (
            from t in _dbContext.AuthTenant.AsNoTracking()
            where t.AuthTenantId == tenantId
            select new TenantListProjection
            {
                Id = t.AuthTenantId,
                Name = t.Name,
                Code = t.Code,
                CreatedDate = t.CreatedDate
            }
        ).FirstAsync(ct);
    }

    private async Task<string?> GenerateTenantCodeAsync()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var year = DateTime.UtcNow.Year;

        Span<byte> buffer = stackalloc byte[4];
        Span<char> randomChars = stackalloc char[4];

        for (int attempt = 0; attempt < 10; attempt++)
        {
            RandomNumberGenerator.Fill(buffer);

            for (int i = 0; i < 4; i++)
                randomChars[i] = chars[buffer[i] % chars.Length];

            var code = $"TNT-{year}-{new string(randomChars)}";

            var exists = await _dbContext.AuthTenant
                .AnyAsync(x => x.Code == code);

            if (!exists)
                return code;
        }

        return null;
    }
}
