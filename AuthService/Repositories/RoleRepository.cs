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

        public Task<AuthRole> CreateAsync(string name, string description, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsByRoleNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _dbContext.AuthRole.AnyAsync(x => x.Name == roleName && !x.IsDeleted, cancellationToken);
        }

        public Task<AuthRole> GetByIdAsync(int userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<(List<AuthRole> Role, int totalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SoftDeleteAsync(int roleId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuthRole?> UpdateAsync(string name, string description, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
