using AuthService.Domain;

namespace AuthService.Repositories
{
    /// <summary>
    /// Repository abstraction for querying roles.
    /// Keeps EF Core details out of the application service.
    /// </summary>
    public interface IRoleRepository
    {
        Task<bool> ExistsByRoleNameAsync(string roleName, CancellationToken cancellationToken = default);
        Task<AuthRole> CreateAsync(string name, string description, CancellationToken cancellationToken = default);
        Task<(List<AuthRole> Role, int totalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<AuthRole> GetByIdAsync(int userId, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<AuthRole?> UpdateAsync(string name, string description, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(int roleId, CancellationToken cancellationToken = default);
    }
}
