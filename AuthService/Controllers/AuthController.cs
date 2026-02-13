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
            var result = await _authService.Login(request);
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
    }

}
