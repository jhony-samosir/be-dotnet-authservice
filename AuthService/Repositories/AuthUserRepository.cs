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

        public async Task<bool> ExistsByEmailAsync(string emailOrUsername, CancellationToken cancellationToken = default)
        {
            return await _dbContext.AuthUser
                .AnyAsync(u => u.Username == emailOrUsername && !u.IsDeleted, cancellationToken);
        }

        public async Task<(AuthUser User, List<string> Roles)> CreateAsync(
            string emailOrUsername,
            string passwordHash,
            IEnumerable<string>? roleNames,
            string? createdBy,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var user = new AuthUser
            {
                Username = emailOrUsername,
                PasswordHash = passwordHash,
                IsActive = true,
                IsLocked = false,
                IsDeleted = false,
                CreatedDate = now,
                CreatedBy = createdBy
            };

            _dbContext.AuthUser.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var rolesList = new List<string>();

            if (roleNames != null)
            {
                var normalizedRoleNames = roleNames
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r.Trim())
                    .Distinct()
                    .ToList();

                if (normalizedRoleNames.Count > 0)
                {
                    var roles = await _dbContext.AuthRole
                        .Where(r => normalizedRoleNames.Contains(r.Name) && !r.IsDeleted)
                        .ToListAsync(cancellationToken);

                    foreach (var role in roles)
                    {
                        var userRole = new AuthUserRole
                        {
                            AuthUserId = user.AuthUserId,
                            AuthRoleId = role.AuthRoleId,
                            AssignedDate = now,
                            AssignedBy = createdBy,
                            IsDeleted = false
                        };

                        _dbContext.AuthUserRole.Add(userRole);
                        rolesList.Add(role.Name);
                    }

                    if (roles.Count > 0)
                    {
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            return (user, rolesList);
        }
    }
}

