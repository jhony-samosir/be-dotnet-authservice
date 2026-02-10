using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Domain;
using AuthService.Repositories;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<AuthUser> _passwordHasher;

        public AuthService(
            IAuthUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher<AuthUser> passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<AuthResponse>> Login(AuthRequest req, CancellationToken cancellationToken = default)
        {
            var (user, roles) = await _userRepository.GetByEmailWithRolesAsync(req.Email, cancellationToken);

            if (user is null || user.IsDeleted || !user.IsActive || user.IsLocked)
            {
                // Do not leak whether the email or the password is wrong.
                return Result<AuthResponse>.Failure("Invalid credentials.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Result<AuthResponse>.Failure("Invalid credentials.");
            }

            // Audit Login Date
            var isAudited = await _userRepository.AuditLoginDate(user.AuthUserId, cancellationToken);
            if (!isAudited)
                return Result<AuthResponse>.Failure("Failure Audit Login Date");

            var (token, expiresInSeconds) = _tokenService.GenerateAccessToken(user, roles);

            var response = new AuthResponse(
                AccessToken: token,
                ExpiresIn: expiresInSeconds,
                TokenType: "Bearer",
                User: new UserInfo(
                    Id: user.AuthUserId,
                    Email: user.Username,
                    Roles: roles
                )
            );

            return Result<AuthResponse>.Success(response);
        }

        public async Task<Result<AuthResponse>> Register(RegisterRequest req, CancellationToken cancellationToken = default)
        {
            var exists = await _userRepository.ExistsByEmailAsync(req.Email, cancellationToken);
            if (exists)
            {
                return Result<AuthResponse>.Failure("Email is already in use.");
            }

            var tempUser = new AuthUser { Username = req.Email };
            var passwordHash = _passwordHasher.HashPassword(tempUser, req.Password);

            var (user, roles) = await _userRepository.CreateAsync(
                emailOrUsername: req.Email,
                passwordHash: passwordHash,
                cancellationToken: cancellationToken);

            var (token, expiresInSeconds) = _tokenService.GenerateAccessToken(user, roles);

            var response = new AuthResponse(
                AccessToken: token,
                ExpiresIn: expiresInSeconds,
                TokenType: "Bearer",
                User: new UserInfo(
                    Id: user.AuthUserId,
                    Email: user.Username,
                    Roles: roles
                )
            );

            return Result<AuthResponse>.Success(response);
        }
    }
}
