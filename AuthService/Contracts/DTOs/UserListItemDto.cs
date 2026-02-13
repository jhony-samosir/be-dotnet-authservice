using System.Text.Json.Serialization;

namespace AuthService.Contracts.DTOs;

/// <summary>
/// User item for list responses (id, email, isActive, isLocked, roles, createdDate).
/// </summary>
public record UserListItemDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("tenant")] string Tenant,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("is_locked")] bool IsLocked,
    [property: JsonPropertyName("created_date")] DateTime CreatedDate
);
