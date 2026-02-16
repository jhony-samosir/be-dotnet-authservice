using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class RoleRepository(DataContext dbContext) : IRoleRepository
{
    private readonly DataContext _dbContext = dbContext;

    public async Task<RoleListProjection> CreateAsync(
        string name,
        string? description,
        CancellationToken ct = default)
    {
        var role = new AuthRole
        {
            Name = name,
            Description = description,
            IsDeleted = false
        };

        _dbContext.AuthRole.Add(role);
        await _dbContext.SaveChangesAsync(ct);

        return await GetProjectionById(role.AuthRoleId, ct);
    }

    public async Task<bool> ExistsByNameAsync(string roleName, CancellationToken ct = default)
    {
        return await _dbContext.AuthRole
            .AsNoTracking()
            .AnyAsync(x => x.Name == roleName && !x.IsDeleted, ct);
    }

    public async Task<RoleListProjection?> GetByIdAsync(
        int roleId,
        CancellationToken ct = default)
    {
        return await (
            from r in _dbContext.AuthRole.AsNoTracking()
            where r.AuthRoleId == roleId && !r.IsDeleted
            select new RoleListProjection
            {
                Id = r.AuthRoleId,
                Name = r.Name,
                Description = r.Description,
                CreatedDate = r.CreatedDate
            }
        ).FirstOrDefaultAsync(ct);
    }

    public async Task<RoleListProjection?> UpdateAsync(
        int roleId,
        string name,
        string? description,
        CancellationToken ct = default)
    {
        var role = await _dbContext.AuthRole
            .FirstOrDefaultAsync(r => r.AuthRoleId == roleId && !r.IsDeleted, ct);

        if (role == null)
            return null;

        role.Name = name;
        role.Description = description;

        await _dbContext.SaveChangesAsync(ct);

        return await GetProjectionById(roleId, ct);
    }

    public async Task<bool> SoftDeleteAsync(int roleId, CancellationToken ct = default)
    {
        var role = await _dbContext.AuthRole
            .FirstOrDefaultAsync(r => r.AuthRoleId == roleId && !r.IsDeleted, ct);

        if (role == null)
            return false;

        role.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PagedResult<RoleListProjection>> GetPagedAsync(
        QueryOptions query,
        CancellationToken ct = default)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize <= 0 || query.PageSize > 100)
            query.PageSize = 10;

        var whitelist = new HashSet<string>
        {
            nameof(RoleListProjection.Name),
            nameof(RoleListProjection.Description),
            nameof(RoleListProjection.CreatedDate)
        };

        IQueryable<RoleListProjection> q =
            from r in _dbContext.AuthRole.AsNoTracking()
            where !r.IsDeleted
            select new RoleListProjection
            {
                Id = r.AuthRoleId,
                Name = r.Name,
                Description = r.Description,
                CreatedDate = r.CreatedDate
            };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = $"%{query.Search}%";
            q = q.Where(x => EF.Functions.ILike(x.Name, s));
        }

        q = q.ApplyQuery(query, whitelist);

        var total = await q.CountAsync(ct);
        var items = await q.ApplyPaging(query).ToListAsync(ct);

        return new PagedResult<RoleListProjection>(
            items,
            total,
            query.Page,
            query.PageSize);
    }

    private async Task<RoleListProjection> GetProjectionById(int roleId, CancellationToken ct)
    {
        return await (
            from r in _dbContext.AuthRole.AsNoTracking()
            where r.AuthRoleId == roleId
            select new RoleListProjection
            {
                Id = r.AuthRoleId,
                Name = r.Name,
                Description = r.Description,
                CreatedDate = r.CreatedDate
            }
        ).FirstAsync(ct);
    }
}
