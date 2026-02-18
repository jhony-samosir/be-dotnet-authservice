using AuthService.Common.Results;
using AuthService.Configuration;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Services.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;

namespace AuthService.Services.Sessions;

public class SessionService(
    DataContext dbContext, 
    ITokenService tokenService, 
    IOptions<JwtSettings> jwtOptions) : ISessionService
{
    private readonly DataContext _dbContext = dbContext;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<AuthSession>> CreateSessionAsync(
        int userId, 
        int tenantId, 
        string refreshToken, 
        string? ipAddress, 
        string? userAgent, 
        string? deviceId,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        if (tenantId <= 0)
        {
             // Fallback or error? For now, let's log and allow if system tenant (0) is valid, 
             // but usually tenant should be > 0. 
             // If 0 is invalid, return failure.
             // return Result<AuthSession>.Failure(new Error(ErrorCode.Validation, "Invalid Tenant ID"));
        }

        var session = new AuthSession
        {
            AuthUserId = userId,
            AuthTenantId = tenantId,
            RefreshTokenHash = _tokenService.HashToken(refreshToken),
            IpAddress = ipAddress,
            UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent, // Truncate if too long
            DeviceId = deviceId,
            DeviceName = deviceName,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTTLDays),
            CreatedAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = userId.ToString(),
            IsCurrent = true,
            IsDeleted = false
        };

        try 
        {
            _dbContext.AuthSession.Add(session);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException dbEx)
        {
            System.Console.WriteLine($"[SessionService] DB Update Error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return Result<AuthSession>.Failure(new Error(ErrorCode.Unknown, "Failed to create session in DB"));
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[SessionService] Critical Error: {ex.Message}");
            return Result<AuthSession>.Failure(new Error(ErrorCode.Unknown, "An unexpected error occurred creating session"));
        }

        return Result<AuthSession>.Success(session);
    }

    public async Task<Result<AuthSession>> ValidateAndRotateSessionAsync(
        string refreshToken, 
        string? ipAddress, 
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var incomingTokenHash = _tokenService.HashToken(refreshToken);

        var session = await _dbContext.AuthSession
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == incomingTokenHash, cancellationToken);
            
        // 1. Session not found?
        if (session == null)
        {
            // Possible attack or just invalid token.
            return Result<AuthSession>.Failure(new Error(ErrorCode.InvalidToken, "Invalid session"));
        }

        // 2. Reuse Detection
        if (session.RevokedAt != null || session.ReplacedByTokenHash != null)
        {
            // REUSE DETECTED!
            // Revoke all sessions for this user family
            await RevokeAllUserSessionsAsync(session.AuthUserId, "Token reuse detected", cancellationToken);
            
            return Result<AuthSession>.Failure(new Error(ErrorCode.InvalidToken, "Token reuse detected. All sessions revoked."));
        }

        // 3. Expiration Check
        if (session.ExpiresAt < DateTime.UtcNow)
        {
             return Result<AuthSession>.Failure(new Error(ErrorCode.InvalidToken, "Session expired"));
        }

        // 4. Rotation Logic
        // Revoke current session
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = "Rotated";
        session.IsCurrent = false;
        
        // Generate new token & hash it for the new session linkage
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashToken(newRefreshToken);
        
        session.ReplacedByTokenHash = newRefreshTokenHash;
        session.LastUsedAt = DateTime.UtcNow;
        session.UpdatedDate = DateTime.UtcNow;
        session.UpdatedBy = session.AuthUserId.ToString();

        // Create new session
        var newSession = new AuthSession
        {
            AuthUserId = session.AuthUserId,
            AuthTenantId = session.AuthTenantId,
            RefreshTokenHash = newRefreshTokenHash,
            IpAddress = ipAddress ?? session.IpAddress,
            UserAgent = userAgent ?? session.UserAgent,
            DeviceId = session.DeviceId,
            DeviceName = session.DeviceName, 
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTTLDays), // Sliding expiration
            CreatedAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = session.AuthUserId.ToString(),
            IsCurrent = true,
            IsDeleted = false
        };
        
        _dbContext.AuthSession.Add(newSession);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        // Returning with Raw Token
        newSession.RefreshTokenHash = newRefreshToken; 
        
        return Result<AuthSession>.Success(newSession);
    }

    public async Task RevokeSessionAsync(string refreshToken, string? reason = null, CancellationToken cancellationToken = default)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var session = await _dbContext.AuthSession
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, cancellationToken);

        if (session != null)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = reason ?? "Manual revocation";
            session.IsCurrent = false;
            session.UpdatedDate = DateTime.UtcNow;
            session.UpdatedBy = "system"; 
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserSessionsAsync(int userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        // Execute Update via ExecuteUpdateAsync for performance if available in .NET 9 + EF Core
        await _dbContext.AuthSession
            .Where(s => s.AuthUserId == userId && s.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RevokedAt, DateTime.UtcNow)
                .SetProperty(x => x.RevokedReason, reason ?? "Revoke all")
                .SetProperty(x => x.IsCurrent, false)
                .SetProperty(x => x.UpdatedDate, DateTime.UtcNow)
                .SetProperty(x => x.UpdatedBy, "system"), // or userId
                cancellationToken);
    }
}
