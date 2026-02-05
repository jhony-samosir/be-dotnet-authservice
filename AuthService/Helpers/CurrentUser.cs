using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AuthService.Helpers
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;
        private ClaimsPrincipal? User => _http.HttpContext?.User;

        private readonly Lazy<Dictionary<string, string>> _claims;

        public CurrentUser(IHttpContextAccessor http)
        {
            _http = http;

            _claims = new Lazy<Dictionary<string, string>>(() =>
                User?.Claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(g => g.Key, g => g.First().Value)
                ?? new());
        }

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public string? Get(string claimType)
        {
            _claims.Value.TryGetValue(claimType, out var val);
            return val;
        }

        private int GetInt(string claim)
        {
            var val = Get(claim);
            return int.TryParse(val, out var v) ? v : 0;
        }

        private bool GetBool(string claim)
        {
            var val = Get(claim);
            return bool.TryParse(val, out var b) && b;
        }

        //private List<int>? GetList(string claim)
        //{
        //    var val = Get(claim);
        //    if (string.IsNullOrEmpty(val)) return null;

        //    return val
        //        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        //        .Select(x => int.TryParse(x, out var n) ? n : 0)
        //        .Where(x => x > 0)
        //        .ToList();
        //}

        private string? GetToken()
        {
            var auth = _http.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(auth)) return null;

            return auth.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        }

        // ========================
        // PROPERTIES
        // ========================

        public int UserId => GetInt(AppConstants.UserId);
        public string? UserName => Get(AppConstants.UserName);
        public string? Email => Get(AppConstants.Email);
        public string? LoginName => Get(AppConstants.LoginName);
        public bool IsAdmin => GetBool(AppConstants.IsAdmin);
        public string? RoleId => Get(AppConstants.RoleId);
        public string? EmailMasked => Get(AppConstants.EmailMasked);
        public string? Token => GetToken();
    }
}
