using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;

namespace AuthService.Services;

/// <summary>
/// Application service for user list, update, and soft delete.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a paged list of non-deleted users.
    /// </summary>
    Task<Result<PagedResult<UserListItemDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user by id. Returns failure if user not found.
    /// </summary>
    Task<Result<UserListItemDto>> UpdateAsync(int userId, UpdateUserRequest request, string? updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a user. Returns failure if user not found or already deleted.
    /// </summary>
    Task<Result<bool>> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default);
}
