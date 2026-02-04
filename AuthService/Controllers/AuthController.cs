using AuthService.Common;
using AuthService.Contracts.Response;
using AuthService.Contracts.Request;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

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
                // Invalid credentials or inactive user: return 401 with a consistent response body.
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
    }

}
