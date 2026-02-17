using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Common.Swagger;
using AuthService.Contracts.DTOs;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Extensions;
using AuthService.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [ApiResult(typeof(ApiResponse<PagedResult<UserListItemDto>>))]
    public async Task<IActionResult> GetList([FromQuery] QueryOptions query, CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetListAsync(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [ApiResult(typeof(ApiResponse<UserListItemDto>))]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.UpdateAsync(id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [ApiResult(typeof(ApiResponse<UserListItemDto>))]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/roles")]
    [ApiResult(typeof(ApiResponse<bool>))]
    public async Task<IActionResult> AssignRoles(int id, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _userService.AssignRolesAsync(id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ApiResult(typeof(ApiResponse<bool>))]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.SoftDeleteAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}
