using System.Text.Json.Serialization;

namespace AuthService.Contracts.Request;

/// <summary>
/// Request to update a user (email, isActive, isLocked, roles).
/// All properties optional; only provided values are updated.
/// </summary>
public class UpdateUserRequest
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("is_active")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("is_locked")]
    public bool? IsLocked { get; set; }

    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }
}
