using AuthService.Common.Results;
using AuthService.Domain;

namespace AuthService.Services.Sessions;

public interface ISessionService
{
    Task<Result<AuthSession>> CreateSessionAsync(
        int userId, 
        int tenantId, 
        string refreshToken, 
        string? ipAddress, 
        string? userAgent, 
        string? deviceId,
        string? deviceName,
        CancellationToken cancellationToken = default);

    Task<Result<AuthSession>> ValidateAndRotateSessionAsync(
        string refreshToken, 
        string? ipAddress, 
        string? userAgent, 
        CancellationToken cancellationToken = default);

    Task RevokeSessionAsync(string refreshToken, string? reason = null, CancellationToken cancellationToken = default);

    Task RevokeAllUserSessionsAsync(int userId, string? reason = null, CancellationToken cancellationToken = default);
}
