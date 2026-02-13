using System.Text.Json.Serialization;
namespace AuthService.Contracts.DTOs;

/// <summary>
/// Role item for list responses.
/// </summary>
public record RoleDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("role_name")] string Name,
    [property: JsonPropertyName("role_description")] string Description
);
