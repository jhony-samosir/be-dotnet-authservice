using AuthService.Data;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IRoleRepository"/>.
    /// Uses the existing <see cref="DataContext"/> and database schema.
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly DataContext _dbContext;

        public RoleRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AuthRole> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
        {
            var role = new AuthRole
            {
                Name = name,
                Description = description,
            };

            _dbContext.AuthRole.Add(role);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return role;
        }

        public async Task<bool> RoleNameAlreadyExists(string roleName, CancellationToken cancellationToken = default)
        {
            return await _dbContext.AuthRole.AnyAsync(x => x.Name == roleName && !x.IsDeleted, cancellationToken);
        }

        public async Task<AuthRole?> GetByIdAsync(int roleId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            return await _dbContext.AuthRole.FirstOrDefaultAsync(x => x.AuthRoleId == roleId, cancellationToken);
        }

        public async Task<(List<AuthRole> Role, int totalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.AuthRole.AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(r => r.AuthRoleId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> SoftDeleteAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _dbContext.AuthRole.FirstOrDefaultAsync(r => r.AuthRoleId == roleId, cancellationToken);

            if (role == null)
                return false;

            _dbContext.Remove(role);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<AuthRole?> UpdateAsync(int roleId, string name, string? description, CancellationToken cancellationToken = default)
        {
            var role = await _dbContext.AuthRole
                .FirstOrDefaultAsync(r => r.AuthRoleId == roleId, cancellationToken);

            if (role == null)
                return null;

            role.Name = name;
            role.Description = description;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return role;
        }
    }
}
