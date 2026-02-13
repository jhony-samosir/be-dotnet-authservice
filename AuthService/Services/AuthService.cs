using AuthService.Common.Results;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Repositories;

namespace AuthService.Services;

public class AuthService(
    IAuthUserRepository userRepository,
    ITokenService tokenService,
    IPasswordService passwordService) : IAuthService
{
    private readonly IAuthUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordService _passwordService = passwordService;

    public async Task<Result<AuthResponse>> Login(
        AuthRequest req,
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

        var (token, expiresInSeconds) =
            _tokenService.GenerateAccessToken(
                user.UserId,
                user.Username,
                user.Roles);

        var response = new AuthResponse(
            AccessToken: token,
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

        var (token, expiresInSeconds) =
            _tokenService.GenerateAccessToken(
                loginUser.UserId,
                loginUser.Username,
                loginUser.Roles);

        var response = new AuthResponse(
            AccessToken: token,
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
}