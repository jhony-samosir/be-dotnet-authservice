using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.DTOs;
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

        /// <summary>
        /// Handles the registration use case:
        /// - ensures the email is not already in use
        /// - creates a new active user with a hashed password
        /// - optionally assigns existing roles
        /// - issues a JWT access token for the new user
        /// </summary>
        public async Task<Result<AuthResponse>> Register(RegisterRequest req, CancellationToken cancellationToken = default)
        {
            // Check if email is already taken
            var exists = await _userRepository.ExistsByEmailAsync(req.Email, cancellationToken);
            if (exists)
            {
                return Result<AuthResponse>.Failure("Email is already in use.");
            }

            // Hash the password using ASP.NET Core Identity hasher
            var tempUser = new AuthUser { Username = req.Email };
            var passwordHash = _passwordHasher.HashPassword(tempUser, req.Password);

            // Create user and assign roles (if provided and existing)
            var (user, roles) = await _userRepository.CreateAsync(
                emailOrUsername: req.Email,
                passwordHash: passwordHash,
                roleNames: req.Roles,
                createdBy: null,
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
