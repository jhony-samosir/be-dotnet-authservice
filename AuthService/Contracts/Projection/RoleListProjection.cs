namespace AuthService.Contracts.Projection;

public sealed class RoleListProjection
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public DateTime? CreatedDate { get; init; }
}