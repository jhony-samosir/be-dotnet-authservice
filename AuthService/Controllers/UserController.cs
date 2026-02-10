using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

/// <summary>
/// User management: list (paged), update, soft delete.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetListAsync(page, pageSize, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return BadRequest(new ApiResponse<PagedResult<UserListItemDto>>(false, result.Error ?? "Failed to get list.", null));

        return Ok(new ApiResponse<PagedResult<UserListItemDto>>(true, "OK", result.Value));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFound(new ApiResponse<UserListItemDto>(result.IsSuccess, result.Error ?? "User not found.", null));
            return BadRequest(new ApiResponse<UserListItemDto>(result.IsSuccess, result.Error ?? "Update failed.", null));
        }

        return Ok(new ApiResponse<UserListItemDto>(result.IsSuccess, "Updated", result.Value));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.SoftDeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true || result.Error?.Contains("already deleted") == true)
                return NotFound(new ApiResponse<bool>(result.IsSuccess, result.Error ?? "User not found or already deleted.", false));
            return BadRequest(new ApiResponse<bool>(result.IsSuccess, result.Error ?? "Delete failed.", false));
        }

        return Ok(new ApiResponse<bool>(result.IsSuccess, "Deleted", true));
    }
}
