using AuthService.Common.Swagger;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Extensions;
using AuthService.Services.Auths;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        
        // Helper to get IP and UserAgent
        private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
        private string? UserAgent => Request.Headers.UserAgent.ToString();

        [AllowAnonymous]
        [HttpPost("login")]
        [ApiResult(typeof(ApiResponse<AuthResponse>))]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var result = await _authService.Login(request, IpAddress, UserAgent);
            return result.ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ApiResult(typeof(ApiResponse<AuthResponse>))]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request);
            return result.ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [ApiResult(typeof(ApiResponse<AuthResponse>))]
        public async Task<IActionResult> RefreshToken()
        {
            // Token is read from Cookie inside service
            var result = await _authService.RefreshToken(string.Empty, IpAddress, UserAgent);
            return result.ToActionResult();
        }

        [HttpPost("logout")]
        [ApiResult(typeof(ApiResponse<bool>))]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.RevokeToken(string.Empty);
            return result.ToActionResult();
        }
    }
}
