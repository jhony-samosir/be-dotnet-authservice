namespace AuthService.Middlewares;

/// <summary>
/// Configures the HTTP request pipeline: Swagger, HTTPS, Authentication, Authorization, Controllers.
/// </summary>
public static class PipelineMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware in the correct order and maps controllers.
    /// Call this after building the WebApplication.
    /// </summary>
    public static WebApplication UseAuthPipeline(this WebApplication app)
    {
        // Security headers first so they apply to all responses including errors
        app.UseSecurityHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Adds enterprise security headers middleware (CORS-related, cache-control, HSTS, X-Frame-Options, etc.).
    /// </summary>
    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }
}
