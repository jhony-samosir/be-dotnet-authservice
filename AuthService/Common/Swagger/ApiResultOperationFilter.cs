using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthService.Common.Swagger;

public class ApiResultOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttr = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<ApiResultAttribute>()
            .Any();

        if (!hasAttr)
            return;

        operation.Responses.TryAdd("400", new OpenApiResponse
        {
            Description = "Bad Request"
        });

        operation.Responses.TryAdd("404", new OpenApiResponse
        {
            Description = "Not Found"
        });

        operation.Responses.TryAdd("409", new OpenApiResponse
        {
            Description = "Conflict"
        });

        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized"
        });

        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Forbidden"
        });

        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Server Error"
        });
    }
}
