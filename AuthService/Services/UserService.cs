using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Repositories;

namespace AuthService.Services;

/// <summary>
/// Implements user list, update, and soft delete using the user repository. <see cref="IUserService"/>.
/// </summary>
public class UserService : IUserService
{
    private readonly IAuthUserRepository _userRepository;

    public UserService(IAuthUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserListItemDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (items, totalCount) = await _userRepository.GetPagedAsync(page, pageSize, cancellationToken);

        var dtos = items.Select(x => new UserListItemDto(
            Id: x.User.AuthUserId,
            Username: x.User.Username,
            Email: x.User.Email,
            IsActive: x.User.IsActive,
            IsLocked: x.User.IsLocked,
            Roles: x.Roles,
            CreatedDate: x.User.CreatedDate
        )).ToList();

        var paged = new PagedResult<UserListItemDto>(dtos, totalCount, page, pageSize);
        return Result<PagedResult<UserListItemDto>>.Success(paged);
    }

    public async Task<Result<UserListItemDto>> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var (user, roles) = await _userRepository.UpdateAsync(
            userId,
            request.Email,
            request.IsActive,
            request.IsLocked,
            request.Roles,
            cancellationToken);

        if (user == null)
            return Result<UserListItemDto>.Failure("User not found.");

        var dto = new UserListItemDto(
            Id: user.AuthUserId,
            Username: user.Username,
            Email: user.Email,
            IsActive: user.IsActive,
            IsLocked: user.IsLocked,
            Roles: roles,
            CreatedDate: user.CreatedDate
        );
        return Result<UserListItemDto>.Success(dto);
    }

    public async Task<Result<bool>> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deleted = await _userRepository.SoftDeleteAsync(userId, cancellationToken);
        if (!deleted)
            return Result<bool>.Failure("User not found or already deleted.");
        return Result<bool>.Success(true);
    }
}
