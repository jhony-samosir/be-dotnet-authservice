using System.Text.Json.Serialization;

namespace AuthService.Contracts.Request;

public record RefreshTokenRequest(
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
