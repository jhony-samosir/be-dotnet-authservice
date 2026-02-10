using AuthService.Common;
using AuthService.Contracts.Request;
using AuthService.Contracts.Response;
using AuthService.Repositories;

namespace AuthService.Services;

/// <summary>
/// Implements role list, update, and soft delete using the role repository. <see cref="IRoleService"/>.
/// </summary>
public class RoleService(IRoleRepository roleRepository) : IRoleService
{
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<Result<RoleListItemDto>> CreateAsync(RoleRequest request, CancellationToken cancellationToken = default)
    {
        var isRoleNameExists = await _roleRepository.RoleNameAlreadyExists(
            request.Name,
            cancellationToken);

        if (isRoleNameExists)
            return Result<RoleListItemDto>.Failure("Role name already exists");

        var role = await _roleRepository.CreateAsync(
            request.Name,
            request.Description,
            cancellationToken);

        if (role == null)
            return Result<RoleListItemDto>.Failure("Failed to create role");

        var result = new RoleListItemDto(
                role.AuthRoleId,
                role.Name,
                role.Description ?? "N/A",
                role.CreatedDate
            );

        return Result<RoleListItemDto>.Success(result);
    }

    public async Task<Result<PagedResult<RoleListItemDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (items, totalCount) = await _roleRepository.GetPagedAsync(page, pageSize, cancellationToken);

        var dtos = items.Select(x => new RoleListItemDto(
            Id: x.AuthRoleId,
            Name: x.Name, 
            Description: x.Description ?? "N/A",
            CreatedDate: x.CreatedDate
        )).ToList();

        var paged = new PagedResult<RoleListItemDto>(dtos, totalCount, page, pageSize);
        return Result<PagedResult<RoleListItemDto>>.Success(paged);
    }

    public async Task<Result<bool>> SoftDeleteAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var deleted = await _roleRepository.SoftDeleteAsync(roleId, cancellationToken);
        if (!deleted)
            return Result<bool>.Failure("Role not found or already deleted.");
        return Result<bool>.Success(true);
    }

    public async Task<Result<RoleListItemDto>> UpdateAsync(int roleId, RoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.UpdateAsync(roleId, request.Name, request.Description, cancellationToken);

        if (role == null)
            return Result<RoleListItemDto>.Failure("Role not found.");

        var dto = new RoleListItemDto(
            Id: role.AuthRoleId,
            Name: role.Name,
            Description: role.Description ?? "N/A",
            CreatedDate: role.CreatedDate
        );
        return Result<RoleListItemDto>.Success(dto);
    }
}
