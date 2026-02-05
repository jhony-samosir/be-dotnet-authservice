using AuthService.Configuration;
using Microsoft.Extensions.Options;

namespace AuthService.Middlewares;

/// <summary>
/// Enterprise-level security headers middleware.
/// Adds standard security and cache-control headers to every response.
/// Should be registered early in the pipeline so all responses (including errors) get the headers.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    private const string HeaderAccessControlAllowHeaders = "Access-Control-Allow-Headers";
    private const string HeaderAccessControlAllowMethods = "Access-Control-Allow-Methods";
    private const string HeaderCacheControl = "Cache-Control";
    private const string HeaderExpires = "Expires";
    private const string HeaderPragma = "Pragma";
    private const string HeaderPermissionsPolicy = "Permissions-Policy";
    private const string HeaderReferrerPolicy = "Referrer-Policy";
    private const string HeaderServer = "Server";
    private const string HeaderStrictTransportSecurity = "Strict-Transport-Security";
    private const string HeaderXContentTypeOptions = "X-Content-Type-Options";
    private const string HeaderXFrameOptions = "X-Frame-Options";

    public SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityHeadersOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;

        if (_options.RemoveServerHeader)
        {
            response.OnStarting(() =>
            {
                if (response.Headers.ContainsKey(HeaderServer))
                    response.Headers.Remove(HeaderServer);
                return Task.CompletedTask;
            });
        }

        if (!string.IsNullOrEmpty(_options.AccessControlAllowHeaders))
            response.Headers[HeaderAccessControlAllowHeaders] = _options.AccessControlAllowHeaders;
        if (!string.IsNullOrEmpty(_options.AccessControlAllowMethods))
            response.Headers[HeaderAccessControlAllowMethods] = _options.AccessControlAllowMethods;
        if (!string.IsNullOrEmpty(_options.CacheControl))
            response.Headers[HeaderCacheControl] = _options.CacheControl;
        if (!string.IsNullOrEmpty(_options.Expires))
            response.Headers[HeaderExpires] = _options.Expires;
        if (!string.IsNullOrEmpty(_options.Pragma))
            response.Headers[HeaderPragma] = _options.Pragma;
        if (!string.IsNullOrEmpty(_options.PermissionsPolicy))
            response.Headers[HeaderPermissionsPolicy] = _options.PermissionsPolicy;
        if (!string.IsNullOrEmpty(_options.ReferrerPolicy))
            response.Headers[HeaderReferrerPolicy] = _options.ReferrerPolicy;
        if (!string.IsNullOrEmpty(_options.XContentTypeOptions))
            response.Headers[HeaderXContentTypeOptions] = _options.XContentTypeOptions;
        if (!string.IsNullOrEmpty(_options.XFrameOptions))
            response.Headers[HeaderXFrameOptions] = _options.XFrameOptions;

        if (_options.StrictTransportSecurityMaxAgeSeconds > 0 && context.Request.IsHttps)
        {
            var hsts = $"max-age={_options.StrictTransportSecurityMaxAgeSeconds}";
            if (_options.StrictTransportSecurityIncludeSubDomains)
                hsts += "; includeSubDomains";
            if (_options.StrictTransportSecurityPreload)
                hsts += "; preload";
            response.Headers[HeaderStrictTransportSecurity] = hsts;
        }

        await _next(context);
    }
}
