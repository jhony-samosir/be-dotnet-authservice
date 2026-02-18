using AuthService.Common.Results;
using AuthService.Configuration;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Domain;
using AuthService.Repositories.AuthUsers;
using AuthService.Services.Cookies;
using AuthService.Services.Passwords;
using AuthService.Services.Sessions;
using AuthService.Services.Tokens;
using AuthService.Helpers;
using Microsoft.Extensions.Options;

namespace AuthService.Services.Auths;

public class AuthService(
    IAuthUserRepository userRepository,
    ITokenService tokenService,
    IPasswordService passwordService,
    ISessionService sessionService,
    ICookieService cookieService,
    ICurrentUser currentUser,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly IAuthUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly ICookieService _cookieService = cookieService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<AuthResponse>> Login(
        AuthRequest req,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate User
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

        // 2. Validate Password
        var hash = await _userRepository.GetPasswordHashAsync(
            user.UserId,
            cancellationToken);

        if (string.IsNullOrEmpty(hash) || !_passwordService.Verify(hash, req.Password))
        {
            return Result<AuthResponse>.Failure(
                new Error(ErrorCode.InvalidCredential, "Invalid username or password"));
        }

        await _userRepository.AuditLoginDateAsync(user.UserId, cancellationToken);

        // 3. Generate Tokens
        var (accessToken, expiresInSeconds) = _tokenService.GenerateAccessToken(
            user.UserId,
            user.Email,
            user.Roles);

        var refreshToken = _tokenService.GenerateRefreshToken();

        // 4. Create Session (DB)
        // We use user-agent from request if device info is not specific enough, but here we pass generic strings.
        // The controller should pass specific User-Agent.
        await _sessionService.CreateSessionAsync(
            user.UserId,
            user.AuthTenantId,
            refreshToken,
            ipAddress,
            deviceInfo, // UserAgent
            null, // DeviceId (could be tracked via headers if implemented)
            null, // DeviceName
            cancellationToken);

        // 5. Set Cookie
        _cookieService.SetRefreshTokenCookie(refreshToken);

        // 6. Response (Access Token ONLY)
        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: string.Empty, // Empty string for JSON response
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

        // Auto-login after register
        var (accessToken, expiresInSeconds) = _tokenService.GenerateAccessToken(
             loginUser.UserId,
             loginUser.Email,
             loginUser.Roles);

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _sessionService.CreateSessionAsync(
            loginUser.UserId,
            loginUser.AuthTenantId,
            refreshToken,
            null, 
            null,
            null,
            null,
            cancellationToken);

        _cookieService.SetRefreshTokenCookie(refreshToken);

        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: string.Empty,
            ExpiresIn: expiresInSeconds,
            TokenType: "Bearer",
            User: new UserInfo(
                Id: loginUser.UserId,
                Email: loginUser.Email,
                Roles: loginUser.Roles
            )
        );

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> RefreshToken(
        string token, // Ignored, we use cookie
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Get Token from Cookie
        var refreshToken = _cookieService.GetRefreshTokenFromCookie();
        
        if (string.IsNullOrEmpty(refreshToken))
        {
             return Result<AuthResponse>.Failure(new Error(ErrorCode.InvalidToken, "No refresh token provided"));
        }

        // 2. Validate & Rotate Session
        var sessionResult = await _sessionService.ValidateAndRotateSessionAsync(
            refreshToken,
            ipAddress,
            deviceInfo,
            cancellationToken);

        if (sessionResult.IsFailure)
        {
            // If reuse detected or invalid, we should clear the cookie
            _cookieService.DeleteRefreshTokenCookie();
            return Result<AuthResponse>.Failure(sessionResult.Error);
        }

        var newSession = sessionResult.Value;

        // 3. Set New Cookie
        // Use the raw token stashed in RefreshTokenHash
        var newRefreshToken = newSession?.RefreshTokenHash
            ?? throw new InvalidOperationException("Refresh token missing");

        _cookieService.SetRefreshTokenCookie(newRefreshToken);

        // 4. Generate New Access Token
        var user = await _userRepository.GetByIdWithRolesAsync(newSession.AuthUserId, cancellationToken);
        if (user is null)
        {
             return Result<AuthResponse>.Failure(new Error(ErrorCode.NotFound, "User not found"));
        }

        var (accessToken, expiresInSeconds) = _tokenService.GenerateAccessToken(
            user.UserId,
            user.Email,
            user.Roles);

        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: string.Empty,
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

    public async Task<Result<bool>> RevokeToken(string token, CancellationToken cancellationToken = default)
    {
        // This method usage depends on context. If "Logout" from specific device:
        // We expect the token to be in the cookie.
        
        var refreshToken = _cookieService.GetRefreshTokenFromCookie();
        if (string.IsNullOrEmpty(refreshToken))
        {
            // Fallback: if caller passed a token explicitly (e.g. admin revoking someone else?)
            // But for now let's assume standard logout flow.
            return Result<bool>.Success(true);
        }

        await _sessionService.RevokeSessionAsync(refreshToken, "Logout", cancellationToken);
        _cookieService.DeleteRefreshTokenCookie();

        return Result<bool>.Success(true);
    }
}