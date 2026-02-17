using AuthService.Data;
using AuthService.Domain;
using AuthService.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories.AuthRefreshTokens;

public class AuthRefreshTokenRepository(DataContext context) : IAuthRefreshTokenRepository
{
    private readonly DataContext _context = context;

    public async Task CreateAsync(AuthRefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.AuthRefreshToken.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.AuthRefreshToken
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsDeleted, cancellationToken);
    }

    public async Task RevokeAsync(int id, string? reason = null, CancellationToken cancellationToken = default)
    {
        var token = await _context.AuthRefreshToken.FindAsync([id], cancellationToken);
        if (token != null)
        {
            token.RevokedDate = DateTime.UtcNow;
            _context.AuthRefreshToken.Remove(token);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.AuthRefreshToken
            .Where(t => t.AuthUserId == userId && t.RevokedDate == null)
            .ToListAsync(cancellationToken);

        if (tokens.Count != 0)
        {
            foreach (var token in tokens)
            {
                token.RevokedDate = DateTime.UtcNow;
                _context.AuthRefreshToken.Remove(token);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateAsync(AuthRefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.AuthRefreshToken.Update(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
