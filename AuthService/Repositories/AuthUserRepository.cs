using AuthService.Data;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IAuthUserRepository"/>.
    /// Uses the existing <see cref="DataContext"/> and database schema.
    /// </summary>
    public class AuthUserRepository : IAuthUserRepository
    {
        private readonly DataContext _dbContext;

        public AuthUserRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(AuthUser? User, List<string> Roles)> GetByEmailWithRolesAsync(
            string emailOrUsername,
            CancellationToken cancellationToken = default)
        {
            // Treat the email as the username column in the existing schema.
            var query =
                from user in _dbContext.AuthUser
                where user.Username == emailOrUsername
                      && !user.IsDeleted
                join userRole in _dbContext.AuthUserRole on user.AuthUserId equals userRole.AuthUserId into userRoles
                from ur in userRoles.DefaultIfEmpty()
                where ur == null || !ur.IsDeleted
                join role in _dbContext.AuthRole on ur.AuthRoleId equals role.AuthRoleId into roles
                from r in roles.DefaultIfEmpty()
                where r == null || !r.IsDeleted
                select new
                {
                    User = user,
                    RoleName = r != null ? r.Name : null
                };

            var results = await query.ToListAsync(cancellationToken);

            if (results.Count == 0)
            {
                return (null, new List<string>());
            }

            var userEntity = results[0].User;
            var roleNames = results
                .Where(x => x.RoleName != null)
                .Select(x => x.RoleName!)
                .Distinct()
                .ToList();

            return (userEntity, roleNames);
        }
    }
}

