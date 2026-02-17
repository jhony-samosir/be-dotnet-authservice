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
                ?? []);
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

        private string? GetToken()
        {
            var auth = _http.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(auth)) return null;

            return auth.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        }

        public int UserId => GetInt(AppConstants.UserId);
        public string? UserName => Get(AppConstants.UserName);
        public string? Email => Get(AppConstants.Email);
        public string? RoleId => Get(AppConstants.RoleId);
        public string? Token => GetToken();
    }
}
