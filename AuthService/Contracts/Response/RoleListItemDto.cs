using System.Text.Json.Serialization;
namespace AuthService.Contracts.Response;

/// <summary>
/// Role item for list responses.
/// </summary>
public record RoleListItemDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("role_name")] string Name,
    [property: JsonPropertyName("role_description")] string Description,
    [property: JsonPropertyName("created_date")] DateTime CreatedDate
);
