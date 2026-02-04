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
    }
}

