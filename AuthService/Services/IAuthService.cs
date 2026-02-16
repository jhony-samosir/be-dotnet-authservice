using AuthService.Common.Results;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> Login(
        AuthRequest req,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default);

        Task<Result<AuthResponse>> Register(
            RegisterRequest req,
            CancellationToken cancellationToken = default);

        Task<Result<AuthResponse>> RefreshToken(
            string refreshToken,
            string? ipAddress = null,
            string? deviceInfo = null,
            CancellationToken cancellationToken = default);

        Task<Result<bool>> RevokeToken(
            string refreshToken,
            CancellationToken cancellationToken = default);
    }
}