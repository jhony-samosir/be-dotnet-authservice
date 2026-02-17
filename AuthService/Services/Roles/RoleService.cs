using AuthService.Common.Querying;
using AuthService.Common.Results;
using AuthService.Contracts.DTOs;
using AuthService.Contracts.Request;
using AuthService.Repositories.Roles;

namespace AuthService.Services.Roles;

/// <summary>
/// Implements role list, update, and soft delete using the role repository. <see cref="IRoleService"/>.
/// </summary>
public class RoleService(IRoleRepository roleRepository) : IRoleService
{
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<Result<RoleListItemDto>> CreateAsync(
        RoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var exists = await _roleRepository.ExistsByNameAsync(
            request.Name,
            cancellationToken);

        if (exists)
        {
            return Result<RoleListItemDto>.Failure(
                new Error(ErrorCode.Conflict, "Role name already exists"));
        }

        var role = await _roleRepository.CreateAsync(
            request.Name,
            request.Description,
            cancellationToken);

        var dto = new RoleListItemDto(
            Id: role.Id,
            Name: role.Name,
            Description: role.Description ?? "N/A",
            CreatedDate: role.CreatedDate
        );

        return Result<RoleListItemDto>.Success(dto);
    }

    public async Task<Result<PagedResult<RoleListItemDto>>> GetListAsync(
        QueryOptions query,
        CancellationToken cancellationToken = default)
    {
        var paged = await _roleRepository.GetPagedAsync(query, cancellationToken);

        var dtos = paged.Items.Select(x => new RoleListItemDto(
            Id: x.Id,
            Name: x.Name,
            Description: x.Description ?? "N/A",
            CreatedDate: x.CreatedDate
        )).ToList();

        var result = new PagedResult<RoleListItemDto>(
            dtos,
            paged.TotalCount,
            paged.Page,
            paged.PageSize);

        return Result<PagedResult<RoleListItemDto>>.Success(result);
    }

    public async Task<Result<RoleListItemDto>> UpdateAsync(
        int roleId,
        RoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.UpdateAsync(
            roleId,
            request.Name,
            request.Description,
            cancellationToken);

        if (role == null)
        {
            return Result<RoleListItemDto>.Failure(
                new Error(ErrorCode.NotFound, "Role not found"));
        }

        var dto = new RoleListItemDto(
            Id: role.Id,
            Name: role.Name,
            Description: role.Description ?? "N/A",
            CreatedDate: role.CreatedDate
        );

        return Result<RoleListItemDto>.Success(dto);
    }

    public async Task<Result<RoleListItemDto>> GetByIdAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(
            roleId,
            cancellationToken);

        if (role == null)
        {
            return Result<RoleListItemDto>.Failure(
                new Error(ErrorCode.NotFound, "Role not found"));
        }

        var dto = new RoleListItemDto(
            Id: role.Id,
            Name: role.Name,
            Description: role.Description ?? "N/A",
            CreatedDate: role.CreatedDate
        );

        return Result<RoleListItemDto>.Success(dto);
    }

    public async Task<Result<bool>> SoftDeleteAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _roleRepository.SoftDeleteAsync(
            roleId,
            cancellationToken);

        if (!deleted)
        {
            return Result<bool>.Failure(
                new Error(ErrorCode.NotFound, "Role not found"));
        }

        return Result<bool>.Success(true);
    }
}
