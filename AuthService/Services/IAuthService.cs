using AuthService.Common.Results;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> Login(AuthRequest req, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> Register(RegisterRequest req, CancellationToken cancellationToken = default);
    }
}