using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.Projection;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories.AuthUsers;

public class AuthUserRepository(DataContext dbContext) : IAuthUserRepository
{
    private readonly DataContext _dbContext = dbContext;

    public async Task<UserWithRolesProjection?> GetByEmailWithRolesAsync(
        string emailOrUsername,
        CancellationToken ct = default)
    {
        return await (
            from u in _dbContext.AuthUser.AsNoTracking()
            where (u.Username == emailOrUsername || u.Email == emailOrUsername)
                  && !u.IsDeleted
            select new UserWithRolesProjection
            {
                UserId = u.AuthUserId,
                AuthTenantId = u.AuthTenantId,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                IsLocked = u.IsLocked,

                Roles = (
                    from ur in _dbContext.AuthUserRole
                    join r in _dbContext.AuthRole
                        on ur.AuthRoleId equals r.AuthRoleId
                    where ur.AuthUserId == u.AuthUserId
                          && !ur.IsDeleted
                          && !r.IsDeleted
                    select r.Name
                ).Distinct().ToList()
            }
        ).FirstOrDefaultAsync(ct);
    }

    public async Task<string?> GetPasswordHashAsync(int userId, CancellationToken ct = default)
    {
        return await _dbContext.AuthUser
            .AsNoTracking()
            .Where(x => x.AuthUserId == userId && !x.IsDeleted)
            .Select(x => x.PasswordHash)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsByEmailAsync(string emailOrUsername, CancellationToken ct = default)
    {
        return await _dbContext.AuthUser
            .AsNoTracking()
            .AnyAsync(u =>
                (u.Username == emailOrUsername || u.Email == emailOrUsername)
                && !u.IsDeleted, ct);
    }

    public async Task<UserListProjection> CreateAsync(
        string emailOrUsername,
        string passwordHash,
        CancellationToken ct = default)
    {
        var roleDefault = await _dbContext.AuthRole
            .FirstAsync(x => x.Name == "Default", ct);

        var username = emailOrUsername.Contains('@')
            ? emailOrUsername.Split('@')[0]
            : emailOrUsername;

        var user = new AuthUser
        {
            Username = username,
            Email = emailOrUsername,
            PasswordHash = passwordHash,
            AuthTenantId = 1,
            IsActive = true,
            IsLocked = false,
            IsDeleted = false
        };

        _dbContext.AuthUser.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        _dbContext.AuthUserRole.Add(new AuthUserRole
        {
            AuthUserId = user.AuthUserId,
            AuthRoleId = roleDefault.AuthRoleId
        });

        await _dbContext.SaveChangesAsync(ct);

        return await GetGridProjectionById(user.AuthUserId, ct);
    }

    public async Task<UserListProjection?> UpdateAsync(
        int userId,
        string? email,
        bool? isActive,
        bool? isLocked,
        IReadOnlyList<string>? roleNames,
        CancellationToken ct = default)
    {
        var user = await _dbContext.AuthUser
            .FirstOrDefaultAsync(u => u.AuthUserId == userId && !u.IsDeleted, ct);

        if (user == null)
            return null;

        if (email != null)
        {
            var username = email.Contains('@') ? email.Split('@')[0] : email;
            user.Username = username;
            user.Email = email;
        }

        if (isActive.HasValue)
            user.IsActive = isActive.Value;

        if (isLocked.HasValue)
            user.IsLocked = isLocked.Value;

        if (roleNames != null)
        {
            var existing = await _dbContext.AuthUserRole
                .Where(x => x.AuthUserId == userId && !x.IsDeleted)
                .ToListAsync(ct);

            foreach (var r in existing)
                r.IsDeleted = true;

            var roles = await _dbContext.AuthRole
                .Where(r => roleNames.Contains(r.Name) && !r.IsDeleted)
                .ToListAsync(ct);

            foreach (var role in roles)
            {
                _dbContext.AuthUserRole.Add(new AuthUserRole
                {
                    AuthUserId = userId,
                    AuthRoleId = role.AuthRoleId
                });
            }
        }

        await _dbContext.SaveChangesAsync(ct);

        return await GetGridProjectionById(userId, ct);
    }

    public async Task<UserWithRolesProjection?> GetByIdWithRolesAsync(
        int userId,
        CancellationToken ct = default)
    {
        return await (
            from u in _dbContext.AuthUser.AsNoTracking()
            where u.AuthUserId == userId && !u.IsDeleted
            select new UserWithRolesProjection
            {
                UserId = u.AuthUserId,
                AuthTenantId = u.AuthTenantId,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                IsLocked = u.IsLocked,

                Roles = (
                    from ur in _dbContext.AuthUserRole
                    join r in _dbContext.AuthRole
                        on ur.AuthRoleId equals r.AuthRoleId
                    where ur.AuthUserId == u.AuthUserId
                          && !ur.IsDeleted
                          && !r.IsDeleted
                    select r.Name
                ).Distinct().ToList()
            }
        ).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> SoftDeleteAsync(int userId, CancellationToken ct = default)
    {
        var user = await _dbContext.AuthUser
            .FirstOrDefaultAsync(x => x.AuthUserId == userId && !x.IsDeleted, ct);

        if (user == null)
            return false;

        user.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AuditLoginDateAsync(int userId, CancellationToken ct = default)
    {
        var user = await _dbContext.AuthUser
            .FirstOrDefaultAsync(x => x.AuthUserId == userId && !x.IsDeleted, ct);

        if (user == null)
            return false;

        user.LastLoginDate = DateTime.UtcNow;
        await _dbContext.SaveChangesSkipAuditAsync(ct);
        return true;
    }

    public async Task<PagedResult<UserListProjection>> GetPagedAsync(QueryOptions query, CancellationToken ct = default)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize <= 0 || query.PageSize > 100)
            query.PageSize = 10;

        var whitelist = new HashSet<string>
        {
            nameof(UserListProjection.Username),
            nameof(UserListProjection.Email),
            nameof(UserListProjection.Tenant),
            nameof(UserListProjection.IsActive),
            nameof(UserListProjection.IsLocked),
            nameof(UserListProjection.CreatedDate)
        };

        IQueryable<UserListProjection> q =
            from u in _dbContext.AuthUser.AsNoTracking()
            join t in _dbContext.AuthTenant.AsNoTracking()
                on u.AuthTenantId equals t.AuthTenantId
            where !u.IsDeleted && !t.IsDeleted
            select new UserListProjection
            {
                Id = u.AuthUserId,
                Username = u.Username,
                Email = u.Email,
                Tenant = t.Name,
                IsActive = u.IsActive,
                IsLocked = u.IsLocked,
                CreatedDate = u.CreatedDate
            };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = $"%{query.Search}%";
            q = q.Where(x =>
                EF.Functions.ILike(x.Username, s) ||
                EF.Functions.ILike(x.Email, s));
        }

        q = q.ApplyQuery(query, whitelist);

        var total = await q.CountAsync(ct);
        var items = await q.ApplyPaging(query).ToListAsync(ct);

        return new PagedResult<UserListProjection>(items, total, query.Page, query.PageSize);
    }

    private async Task<UserListProjection> GetGridProjectionById(int userId, CancellationToken ct)
    {
        return await (
            from u in _dbContext.AuthUser.AsNoTracking()
            join t in _dbContext.AuthTenant.AsNoTracking()
                on u.AuthTenantId equals t.AuthTenantId
            where u.AuthUserId == userId
            select new UserListProjection
            {
                Id = u.AuthUserId,
                Username = u.Username,
                Email = u.Email,
                Tenant = t.Name,
                IsActive = u.IsActive,
                IsLocked = u.IsLocked,
                CreatedDate = u.CreatedDate
            }
        ).FirstAsync(ct);
    }
}
