using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(AuthRequest req);
    }
    public class AuthService : IAuthService
    {
        public Task<AuthResponse> Login(AuthRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
