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
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Get Role Default
            var roleDefault = await _dbContext.AuthRole
                .FirstOrDefaultAsync(x => x.Name == "Default", cancellationToken);

            if (roleDefault == null)
            {
                roleDefault = new AuthRole
                {
                    Name = "Default",
                    Description = "Role Default",
                    CreatedBy = "System"
                };

                _dbContext.AuthRole.Add(roleDefault);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // Get username from email
            var username = emailOrUsername.Contains('@')
                ? emailOrUsername.Split('@')[0]
                : emailOrUsername;

            var user = new AuthUser
            {
                Username = username,
                Email = emailOrUsername,
                PasswordHash = passwordHash,
                IsActive = true,
                IsLocked = false,
                IsDeleted = false
            };

            _dbContext.AuthUser.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var userRole = new AuthUserRole
            {
                AuthUserId = user.AuthUserId,
                AuthRoleId = roleDefault.AuthRoleId,
                AssignedBy = "System",
                AssignedDate = now
            };

            _dbContext.AuthUserRole.Add(userRole);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var rolesList = new List<string> { roleDefault.Name };

            return (user, rolesList);
        }

        public async Task<(List<(AuthUser User, List<string> Roles)> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await _dbContext.AuthUser.CountAsync(cancellationToken);

            var userIds = await _dbContext.AuthUser
                .OrderBy(u => u.AuthUserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => u.AuthUserId)
                .ToListAsync(cancellationToken);

            if (userIds.Count == 0)
                return (new List<(AuthUser User, List<string> Roles)>(), totalCount);

            var users = await _dbContext.AuthUser
                .Where(u => userIds.Contains(u.AuthUserId))
                .OrderBy(u => u.AuthUserId)
                .ToListAsync(cancellationToken);

            var roleMap = await (from ur in _dbContext.AuthUserRole
                    join r in _dbContext.AuthRole on ur.AuthRoleId equals r.AuthRoleId
                    where userIds.Contains(ur.AuthUserId) && !ur.IsDeleted && !r.IsDeleted
                    select new { ur.AuthUserId, r.Name })
                .ToListAsync(cancellationToken);

            var rolesByUser = roleMap
                .GroupBy(x => x.AuthUserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

            var items = users.Select(u => (User: u, Roles: rolesByUser.GetValueOrDefault(u.AuthUserId, []))).ToList();
            return (items, totalCount);
        }

        public async Task<(AuthUser? User, List<string> Roles)> GetByIdWithRolesAsync(
            int userId,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.AuthUser.AsQueryable();
            if (includeDeleted)
                query = query.IgnoreQueryFilters();

            var user = await query.FirstOrDefaultAsync(u => u.AuthUserId == userId, cancellationToken);
            if (user == null)
                return (null, new List<string>());

            var roleRows = await (from ur in _dbContext.AuthUserRole
                    join r in _dbContext.AuthRole on ur.AuthRoleId equals r.AuthRoleId
                    where ur.AuthUserId == userId && !ur.IsDeleted && !r.IsDeleted
                    select r.Name)
                .ToListAsync(cancellationToken);

            return (user, roleRows);
        }

        public async Task<(AuthUser? User, List<string> Roles)> UpdateAsync(
            int userId,
            string? email,
            bool? isActive,
            bool? isLocked,
            IReadOnlyList<string>? roleNames,
            CancellationToken cancellationToken = default)
        {
            var (user, _) = await GetByIdWithRolesAsync(userId, includeDeleted: false, cancellationToken);
            if (user == null)
                return (null, new List<string>());

            if (email != null)
                user.Username = email;
            if (isActive.HasValue)
                user.IsActive = isActive.Value;
            if (isLocked.HasValue)
                user.IsLocked = isLocked.Value;

            if (roleNames != null)
            {
                var existing = await _dbContext.AuthUserRole
                    .Where(ur => ur.AuthUserId == userId && !ur.IsDeleted)
                    .ToListAsync(cancellationToken);
                foreach (var ur in existing)
                    ur.IsDeleted = true;
                var now = DateTime.UtcNow;
                var normalized = roleNames.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct().ToList();
                if (normalized.Count > 0)
                {
                    var roles = await _dbContext.AuthRole
                        .Where(r => normalized.Contains(r.Name) && !r.IsDeleted)
                        .ToListAsync(cancellationToken);
                    foreach (var role in roles)
                    {
                        _dbContext.AuthUserRole.Add(new AuthUserRole
                        {
                            AuthUserId = userId,
                            AuthRoleId = role.AuthRoleId,
                            AssignedDate = now,
                            IsDeleted = false
                        });
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var rolesList = await (from ur in _dbContext.AuthUserRole
                    join r in _dbContext.AuthRole on ur.AuthRoleId equals r.AuthRoleId
                    where ur.AuthUserId == userId && !ur.IsDeleted && !r.IsDeleted
                    select r.Name)
                .ToListAsync(cancellationToken);

            return (user, rolesList);
        }

        public async Task<bool> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.AuthUser
                .FirstOrDefaultAsync(u => u.AuthUserId == userId && !u.IsDeleted, cancellationToken);
            if (user == null)
                return false;

            _dbContext.AuthUser.Remove(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

