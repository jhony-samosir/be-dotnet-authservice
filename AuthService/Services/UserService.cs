using AuthService.Common.Querying;
using AuthService.Contracts.Request;
using AuthService.Contracts.DTOs;
using AuthService.Repositories;
using AuthService.Common.Results;

namespace AuthService.Services;

public class UserService(IAuthUserRepository userRepository) : IUserService
{
    private readonly IAuthUserRepository _userRepository = userRepository;

    public async Task<Result<PagedResult<UserListItemDto>>> GetListAsync(
        QueryOptions query,
        CancellationToken cancellationToken = default)
    {
        var paged = await _userRepository.GetPagedAsync(query, cancellationToken);

        var dtos = paged.Items.Select(x => new UserListItemDto(
            Id: x.Id,
            Username: x.Username,
            Email: x.Email,
            Tenant: x.Tenant,
            IsActive: x.IsActive,
            IsLocked: x.IsLocked,
            CreatedDate: x.CreatedDate
        )).ToList();

        var result = new PagedResult<UserListItemDto>(
            dtos,
            paged.TotalCount,
            paged.Page,
            paged.PageSize);

        return Result<PagedResult<UserListItemDto>>.Success(result);
    }

    public async Task<Result<UserListItemDto>> UpdateAsync(
        int userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.UpdateAsync(
            userId,
            request.Email,
            request.IsActive,
            request.IsLocked,
            request.Roles,
            cancellationToken);

        if (user == null)
            return Result<UserListItemDto>.Failure(
                new Error(ErrorCode.NotFound, "User not found"));

        var dto = new UserListItemDto(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            Tenant: user.Tenant,
            IsActive: user.IsActive,
            IsLocked: user.IsLocked,
            CreatedDate: user.CreatedDate
        );

        return Result<UserListItemDto>.Success(dto);
    }

    public async Task<Result<UserListItemDto>> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);

        if (user == null)
            return Result<UserListItemDto>.Failure(
                new Error(ErrorCode.NotFound, "User not found"));

        var dto = new UserListItemDto(
            Id: user.UserId,
            Username: user.Username,
            Email: user.Email,
            Tenant: "", // optional: join tenant if needed
            IsActive: user.IsActive,
            IsLocked: user.IsLocked,
            CreatedDate: DateTime.UtcNow // or remove if not needed
        );

        return Result<UserListItemDto>.Success(dto);
    }

    public async Task<Result<bool>> SoftDeleteAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _userRepository.SoftDeleteAsync(userId, cancellationToken);

        if (!deleted)
            return Result<bool>.Failure(
                new Error(ErrorCode.NotFound, "User not found"));

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> AssignRolesAsync(
        int userId,
        AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.UpdateAsync(
            userId,
            null,
            null,
            null,
            request.Roles,
            cancellationToken);

        if (user == null)
            return Result<bool>.Failure(
                new Error(ErrorCode.NotFound, "User not found"));

        return Result<bool>.Success(true);
    }
}
