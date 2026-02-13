using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.DTOs;
using AuthService.Contracts.Request;

namespace AuthService.Services;

/// <summary>
/// Application service for user list, update, and soft delete.
/// </summary>
public interface IUserService
{
    Task<Result<PagedResult<UserListItemDto>>> GetListAsync(QueryOptions query, CancellationToken cancellationToken = default);
    Task<Result<UserListItemDto>> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<UserListItemDto>> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> AssignRolesAsync(int userId, AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default);
}
