namespace AuthService.Configuration
{
    /// <summary>
    /// Strongly-typed configuration for JWT settings.
    /// Bound from configuration section "Jwt" using the IOptions pattern.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty;
        public int AccessTokenMinutes { get; set; } = 60;
    }
}

