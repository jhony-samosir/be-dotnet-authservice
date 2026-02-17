using AuthService.Common.Results;
using AuthService.Configuration;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Domain;
using AuthService.Repositories.AuthRefreshTokens;
using AuthService.Repositories.AuthUsers;
using AuthService.Services.Passwords;
using AuthService.Services.Tokens;
using Microsoft.Extensions.Options;

namespace AuthService.Services.Auths;

public class AuthService(
    IAuthUserRepository userRepository,
    ITokenService tokenService,
    IPasswordService passwordService,
    IAuthRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly IAuthUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IAuthRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<AuthResponse>> Login(
        AuthRequest req,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository
            .GetByEmailWithRolesAsync(req.Email, cancellationToken);

        if (user is null)
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.InvalidCredential, "Invalid username or password"));
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.Forbidden, "User inactive"));
        }

        if (user.IsLocked)
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.Forbidden, "User locked"));
        }

        // ambil hash password dari repo
        var hash = await _userRepository.GetPasswordHashAsync(
            user.UserId,
            cancellationToken);

        if (string.IsNullOrEmpty(hash) || !_passwordService.Verify(hash, req.Password))
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.InvalidCredential, "Invalid username or password"));
        }

        await _userRepository.AuditLoginDateAsync(user.UserId, cancellationToken);

        return await GenerateAuthResponseAsync(user, ipAddress, deviceInfo, cancellationToken);
    }

    public async Task<Result<AuthResponse>> Register(
        RegisterRequest req,
        CancellationToken cancellationToken = default)
    {
        var exists = await _userRepository
            .ExistsByEmailAsync(req.Email, cancellationToken);

        if (exists)
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.Conflict, "Email already registered"));
        }

        var passwordHash = _passwordService.Hash(req.Password);

        var created = await _userRepository.CreateAsync(
            emailOrUsername: req.Email,
            passwordHash: passwordHash,
            cancellationToken: cancellationToken);

        var loginUser = await _userRepository
            .GetByIdWithRolesAsync(created.Id, cancellationToken);

        if (loginUser == null)
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.Unknown, "User creation failed"));
        }

        // For register, we can also generate refresh token immediately
        return await GenerateAuthResponseAsync(loginUser, null, null, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshToken(
        string token,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(token, cancellationToken);

        if (existingToken is null)
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.InvalidToken, "Invalid refresh token"));
        }

        if (existingToken.RevokedDate != null)
        {
            // Token revocation check - Security feature: Token Reuse Detection
            // If a revoked token is used, it might mean it was stolen.
            // We should revoke all refresh tokens for this user family to be safe.
            await _refreshTokenRepository.RevokeAllByUserIdAsync(existingToken.AuthUserId, cancellationToken);
            
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.InvalidToken, "Token has been revoked"));
        }

        if (existingToken.ExpiredDate < DateTime.UtcNow)
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.InvalidToken, "Token expired"));
        }

        // Token Rotation: Revoke the used refresh token
        await _refreshTokenRepository.RevokeAsync(existingToken.AuthRefreshTokenId, "Replaced by new token", cancellationToken);

        var user = await _userRepository.GetByIdWithRolesAsync(existingToken.AuthUserId, cancellationToken);
        if (user is null)
        {
             return Result<AuthResponse>.Failure(
                new Error(ErrorCode.NotFound, "User not found"));
        }

        return await GenerateAuthResponseAsync(user, ipAddress, deviceInfo, cancellationToken);
    }

    public async Task<Result<bool>> RevokeToken(string token, CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(token, cancellationToken);

        if (existingToken is null)
        {
             return Result<bool>.Failure(
                new Error(ErrorCode.NotFound, "Token not found"));
        }
        
        // Ensure we are not double revoking if already revoked, although RevokeAsync handles idempotent logic usually.
        if (existingToken.RevokedDate == null) 
        {
             await _refreshTokenRepository.RevokeAsync(existingToken.AuthRefreshTokenId, "Manual revocation", cancellationToken);
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<AuthResponse>> GenerateAuthResponseAsync(
        Contracts.Projection.UserWithRolesProjection user, 
        string? ipAddress, 
        string? deviceInfo, 
        CancellationToken cancellationToken)
    {
        var (accessToken, expiresInSeconds) = _tokenService.GenerateAccessToken(
            user.UserId,
            user.Username,
            user.Roles);

        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new AuthRefreshToken
        {
            AuthUserId = user.UserId,
            Token = refreshToken,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            ExpiredDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTTLDays),
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Username, // or System
            IsDeleted = false
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity, cancellationToken);

        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresIn: expiresInSeconds,
            TokenType: "Bearer",
            User: new UserInfo(
                Id: user.UserId,
                Email: user.Email,
                Roles: user.Roles
            )
        );

        return Result<AuthResponse>.Success(response);
    }
}