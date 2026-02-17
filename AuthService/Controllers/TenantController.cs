using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Common.Swagger;
using AuthService.Contracts.Projection;
using AuthService.Contracts.Response;
using AuthService.Extensions;
using AuthService.Services.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

/// <summary>
/// Tenant management: list (paged), update, soft delete.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantController(ITenantService tenantService) : ControllerBase
{
    private readonly ITenantService _tenantService = tenantService;

    [HttpPost]
    [ApiResult(typeof(ApiResponse<TenantListProjection>))]
    public async Task<IActionResult> CreateAsync([FromBody] string name, CancellationToken ct = default)
    {
        var result = await _tenantService.CreateAsync(name, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [ApiResult(typeof(ApiResponse<TenantListProjection>))]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _tenantService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    [ApiResult(typeof(ApiResponse<PagedResult<TenantListProjection>>))]
    public async Task<IActionResult> GetListAsync([FromQuery] QueryOptions query, CancellationToken ct = default)
    {
        var result = await _tenantService.GetListAsync(query, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [ApiResult(typeof(ApiResponse<bool>))]
    public async Task<IActionResult> SoftDeleteAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _tenantService.SoftDeleteAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [ApiResult(typeof(ApiResponse<TenantListProjection>))]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] string name, CancellationToken ct = default)
    {
        var result = await _tenantService.UpdateAsync(id, name, ct);
        return result.ToActionResult();
    }
}