using System.Text.Json.Serialization;

namespace AuthService.Contracts.Response
{
    /// <summary>
    /// Authentication response following a typical OAuth2-style
    /// structure (access_token, expires_in, token_type, user).
    /// 
    /// JsonPropertyName attributes are used to match the required
    /// snake_case fields in the API response.
    /// </summary>
    public record UserInfo(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles
    );

    public record AuthResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("user")] UserInfo User
    );
}

