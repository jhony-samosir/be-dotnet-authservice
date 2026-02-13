using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Common.Swagger;
using AuthService.Contracts.DTOs;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Extensions;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

/// <summary>
/// Role management: list (paged), update, soft delete.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController(IRoleService roleService) : Controller
{
    private readonly IRoleService _roleService = roleService;

    [HttpPost]
    [ApiResult(typeof(ApiResponse<PagedResult<RoleListItemDto>>))]
    public async Task<IActionResult> CreateRole([FromBody] RoleRequest request, CancellationToken ct)
    {
        var result = await _roleService.CreateAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    [ApiResult(typeof(ApiResponse<PagedResult<RoleListItemDto>>))]
    public async Task<IActionResult> GetList([FromQuery] QueryOptions query, CancellationToken cancellationToken = default)
    {
        var result = await _roleService.GetListAsync(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [ApiResult(typeof(ApiResponse<PagedResult<RoleListItemDto>>))]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _roleService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [ApiResult(typeof(ApiResponse<PagedResult<RoleListItemDto>>))]
    public async Task<IActionResult> Update(int id, [FromBody] RoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ApiResult(typeof(ApiResponse<bool>))]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken = default)
    {
        var result = await _roleService.SoftDeleteAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}
