using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;

namespace AuthService.Services;

/// <summary>
/// Application service for role list, update, and soft delete.
/// </summary>
public interface IRoleService
{
    Task<Result<RoleListItemDto>> CreateAsync(RoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<RoleListItemDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<RoleListItemDto>> UpdateAsync(int roleId, RoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> SoftDeleteAsync(int roleId, CancellationToken cancellationToken = default);
}
