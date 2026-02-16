using System.Text.Json.Serialization;

namespace AuthService.Contracts.Request;

public record RevokeTokenRequest(
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
