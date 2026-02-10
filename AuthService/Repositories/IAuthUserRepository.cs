using AuthService.Domain;

namespace AuthService.Repositories
{
    /// <summary>
    /// Repository abstraction for querying authentication users and their roles.
    /// Keeps EF Core details out of the application service.
    /// </summary>
    public interface IAuthUserRepository
    {
        Task<(AuthUser? User, List<string> Roles)> GetByEmailWithRolesAsync(string emailOrUsername, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string emailOrUsername, CancellationToken cancellationToken = default);
        Task<(AuthUser User, List<string> Roles)> CreateAsync(string emailOrUsername, string passwordHash, CancellationToken cancellationToken = default);
        Task<(List<(AuthUser User, List<string> Roles)> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<(AuthUser? User, List<string> Roles)> GetByIdWithRolesAsync(int userId, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<(AuthUser? User, List<string> Roles)> UpdateAsync(int userId, string? email, bool? isActive, bool? isLocked, IReadOnlyList<string>? roleNames, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> AuditLoginDate(int userId, CancellationToken cancellationToken = default);
    }
}

