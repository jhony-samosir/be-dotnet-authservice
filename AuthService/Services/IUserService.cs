using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;

namespace AuthService.Services;

/// <summary>
/// Application service for user list, update, and soft delete.
/// </summary>
public interface IUserService
{
    Task<Result<PagedResult<UserListItemDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<UserListItemDto>> UpdateAsync(int userId, UpdateUserRequest request, string? updatedBy, CancellationToken cancellationToken = default);
    Task<Result<bool>> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default);
}
