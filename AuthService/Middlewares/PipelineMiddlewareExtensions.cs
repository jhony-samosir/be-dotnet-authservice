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
}
