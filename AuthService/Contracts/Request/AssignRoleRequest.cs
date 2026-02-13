namespace AuthService.Contracts.Request;

public record AssignRoleRequest(
    List<string> Roles
);
