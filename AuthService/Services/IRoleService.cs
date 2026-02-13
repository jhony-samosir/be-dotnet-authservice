using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.DTOs;
using AuthService.Contracts.Request;

namespace AuthService.Services;

/// <summary>
/// Application service for role list, update, and soft delete.
/// </summary>
public interface IRoleService
{
    Task<Result<RoleListItemDto>> CreateAsync(
        RoleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<RoleListItemDto>>> GetListAsync(
        QueryOptions query,
        CancellationToken cancellationToken = default);

    Task<Result<RoleListItemDto>> GetByIdAsync(
        int roleId,
        CancellationToken cancellationToken = default);

    Task<Result<RoleListItemDto>> UpdateAsync(
        int roleId,
        RoleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> SoftDeleteAsync(
        int roleId,
        CancellationToken cancellationToken = default);
}
