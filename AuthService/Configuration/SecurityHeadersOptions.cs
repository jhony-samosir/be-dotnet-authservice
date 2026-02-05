namespace AuthService.Configuration;

/// <summary>
/// Options for enterprise security headers middleware.
/// Can be bound from configuration (e.g. "SecurityHeaders" section) or use defaults.
/// </summary>
public class SecurityHeadersOptions
{
    public const string SectionName = "SecurityHeaders";

    /// <summary>
    /// Comma-separated list of allowed request headers for CORS.
    /// </summary>
    public string AccessControlAllowHeaders { get; set; } =
        "Accept,Authorization,Content-Type,Dnt,Referer,Sec-Ch-Ua,Sec-Ch-Ua-Mobile,Sec-Ch-Ua-Platform,User-Agent,X-Csrf-Token,X-OTP";

    /// <summary>
    /// Comma-separated list of allowed HTTP methods for CORS.
    /// </summary>
    public string AccessControlAllowMethods { get; set; } = "GET,POST,PUT,PATCH,DELETE,OPTIONS";

    /// <summary>
    /// Cache-Control value for API responses (no-store recommended for auth/sensitive data).
    /// </summary>
    public string CacheControl { get; set; } = "no-store,no-cache,must-revalidate,max-age=0";

    /// <summary>
    /// Expires header value (past date to prevent caching).
    /// </summary>
    public string Expires { get; set; } = "Sat, 01 Jan 2000 00:00:00 GMT";

    /// <summary>
    /// Pragma no-cache for legacy clients.
    /// </summary>
    public string Pragma { get; set; } = "no-cache";

    /// <summary>
    /// Permissions-Policy (formerly Feature-Policy) to restrict browser features.
    /// </summary>
    public string PermissionsPolicy { get; set; } = "camera=(), microphone=(), geolocation=()";

    /// <summary>
    /// Referrer-Policy: controls how much referrer info is sent.
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// X-Content-Type-Options: prevents MIME sniffing.
    /// </summary>
    public string XContentTypeOptions { get; set; } = "nosniff";

    /// <summary>
    /// X-Frame-Options: prevents clickjacking (DENY or SAMEORIGIN).
    /// </summary>
    public string XFrameOptions { get; set; } = "SAMEORIGIN";

    /// <summary>
    /// Strict-Transport-Security (HSTS) max-age in seconds. Set 0 to disable.
    /// Only effective over HTTPS. 7862400 = 91 days.
    /// </summary>
    public int StrictTransportSecurityMaxAgeSeconds { get; set; } = 7862400;

    /// <summary>
    /// Include subdomains in HSTS.
    /// </summary>
    public bool StrictTransportSecurityIncludeSubDomains { get; set; } = true;

    /// <summary>
    /// Add preload directive for HSTS (submit site to browser preload list).
    /// </summary>
    public bool StrictTransportSecurityPreload { get; set; } = true;

    /// <summary>
    /// Remove or obscure Server header (Kestrel, etc.). If true, Server header is not set.
    /// </summary>
    public bool RemoveServerHeader { get; set; } = true;
}
