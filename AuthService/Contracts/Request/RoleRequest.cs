using System.Text.Json.Serialization;

namespace AuthService.Contracts.Request
{
    public class RoleRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
