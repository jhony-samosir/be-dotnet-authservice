using AuthService.Common.Swagger;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Extensions;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [AllowAnonymous]
        [HttpPost("login")]
        [ApiResult(typeof(ApiResponse<AuthResponse>))]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceInfo = Request.Headers.UserAgent.ToString();
            
            var result = await _authService.Login(request, ipAddress, deviceInfo);
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
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceInfo = Request.Headers.UserAgent.ToString();

            var result = await _authService.RefreshToken(request.RefreshToken, ipAddress, deviceInfo);
            return result.ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("revoke")]
        [ApiResult(typeof(ApiResponse<bool>))]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            var result = await _authService.RevokeToken(request.RefreshToken);
            return result.ToActionResult();
        }
    }

}
