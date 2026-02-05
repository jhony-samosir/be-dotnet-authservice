using AuthService.Domain;

namespace AuthService.Repositories
{
    /// <summary>
    /// Repository abstraction for querying authentication users and their roles.
    /// Keeps EF Core details out of the application service.
    /// </summary>
    public interface IAuthUserRepository
    {
        /// <summary>
        /// Finds an active, non-deleted user by email/username and returns the user
        /// together with the list of role names assigned to the user.
        /// 
        /// For this project, the <paramref name="emailOrUsername"/> is mapped to the
        /// <see cref="AuthUser.Username"/> field, which is assumed to be the user's email.
        /// </summary>
        Task<(AuthUser? User, List<string> Roles)> GetByEmailWithRolesAsync(
            string emailOrUsername,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns true if an active (non-deleted) user already exists with the given email/username.
        /// Used to enforce uniqueness during registration.
        /// </summary>
        Task<bool> ExistsByEmailAsync(string emailOrUsername, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user with the specified email and password hash and optionally assigns roles.
        /// Returns the created user together with the roles actually assigned.
        /// </summary>
        Task<(AuthUser User, List<string> Roles)> CreateAsync(
            string emailOrUsername,
            string passwordHash,
            IEnumerable<string>? roleNames,
            string? createdBy,
            CancellationToken cancellationToken = default);
    }
}

