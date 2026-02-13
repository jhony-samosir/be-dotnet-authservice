namespace AuthService.Contracts.Projection;

public sealed class UserWithRolesProjection
{
    public int UserId { get; init; }
    public string Username { get; init; } = "";
    public string Email { get; init; } = "";
    public bool IsActive { get; init; }
    public bool IsLocked { get; init; }

    public List<string> Roles { get; init; } = [];
}