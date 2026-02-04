using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.DTOs;
using AuthService.Domain;
using AuthService.Repositories;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> Login(AuthRequest req, CancellationToken cancellationToken = default);
    }
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

        /// <summary>
        /// Handles the login use case:
        /// - validates that the user exists and is active
        /// - verifies the password using ASP.NET Core Identity's password hasher
        /// - generates a JWT access token
        /// 
        /// Business errors such as invalid credentials or inactive user
        /// are returned as failed <see cref="Result{T}"/> rather than exceptions.
        /// </summary>
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
