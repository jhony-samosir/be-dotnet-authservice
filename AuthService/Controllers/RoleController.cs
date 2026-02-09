using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

/// <summary>
/// Role management: list (paged), update, soft delete.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : Controller
{
    private readonly IRoleService _roleService;
    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleListItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RoleListItemDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RoleListItemDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] RoleRequest request, CancellationToken ct)
    {
        var result = await _roleService.CreateAsync(request, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "Role name already exists")
            {
                return Conflict(new ApiResponse<RoleListItemDto>(
                    Success: false,
                    Message: result.Error,
                    Data: null
                ));
            }

            return BadRequest(new ApiResponse<RoleListItemDto>(
                Success: false,
                Message: result.Error ?? "Failed to create role",
                Data: null
            ));
        }

        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<RoleListItemDto>(
                Success: true,
                Message: "Role created successfully",
                Data: result.Value
            ));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RoleListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.GetListAsync(page, pageSize, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
            return BadRequest(new ApiResponse<PagedResult<RoleListItemDto>>(false, result.Error ?? "Failed to get list.", null));

        return Ok(new ApiResponse<PagedResult<RoleListItemDto>>(true, "OK", result.Value));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RoleListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] RoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFound(new ApiResponse<RoleListItemDto>(result.IsSuccess, result.Error ?? "Role not found.", null));
            return BadRequest(new ApiResponse<RoleListItemDto>(result.IsSuccess, result.Error ?? "Update failed.", null));
        }

        return Ok(new ApiResponse<RoleListItemDto>(result.IsSuccess, "Updated", result.Value));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken = default)
    {
        var result = await _roleService.SoftDeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true || result.Error?.Contains("already deleted") == true)
                return NotFound(new ApiResponse<bool>(result.IsSuccess, result.Error ?? "Role not found or already deleted.", false));
            return BadRequest(new ApiResponse<bool>(result.IsSuccess, result.Error ?? "Delete failed.", false));
        }

        return Ok(new ApiResponse<bool>(result.IsSuccess, "Deleted", true));
    }
}
