using AuthService.Domain;

namespace AuthService.Repositories;

/// <summary>
/// Repository abstraction for querying authentication refresh token.
/// Keeps EF Core details out of the application service.
/// </summary>
public interface IAuthRefreshTokenRepository
{
    Task<AuthRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task CreateAsync(AuthRefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(int id, string? reason = null, CancellationToken cancellationToken = default);
    Task RevokeAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(AuthRefreshToken refreshToken, CancellationToken cancellationToken = default);
}
