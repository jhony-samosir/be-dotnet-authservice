namespace AuthService.Middlewares;

/// <summary>
/// Configures the HTTP request pipeline: Swagger, HTTPS, Authentication, Authorization, Controllers.
/// </summary>
public static class PipelineMiddlewareExtensions
{
    public static WebApplication UseAuthPipeline(this WebApplication app)
    {
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

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }
}
