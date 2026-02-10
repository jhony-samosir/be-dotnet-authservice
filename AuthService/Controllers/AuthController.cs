using AuthService.Common;
using AuthService.Contracts.Response;
using AuthService.Contracts.Request;
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
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var result = await _authService.Login(request);

            if (!result.IsSuccess || result.Value is null)
            {
                return Unauthorized(new ApiResponse<AuthResponse>(
                    Success: false,
                    Message: result.Error ?? "Invalid credentials.",
                    Data: null
                ));
            }

            return Ok(new ApiResponse<AuthResponse>(
                Success: true,
                Message: "Login success",
                Data: result.Value
            ));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request);

            if (!result.IsSuccess || result.Value is null)
            {
                return Conflict(new ApiResponse<AuthResponse>(
                    Success: false,
                    Message: result.Error ?? "Registration failed.",
                    Data: null
                ));
            }

            return StatusCode(StatusCodes.Status201Created, new ApiResponse<AuthResponse>(
                Success: true,
                Message: "Registration success",
                Data: result.Value
            ));
        }
    }

}
