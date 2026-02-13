using AuthService.Contracts.Response;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace AuthService.Middlewares;

public sealed class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IWebHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;
    private readonly IWebHostEnvironment _env = env;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // client aborted request (nginx: 499)
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var traceId = ctx.TraceIdentifier;

        // 🔥 structured logging
        _logger.LogError(ex,
            "Unhandled exception. TraceId={TraceId} Path={Path}",
            traceId,
            ctx.Request.Path);

        if (ctx.Response.HasStarted)
        {
            _logger.LogWarning("Response already started, cannot write error body. TraceId={TraceId}", traceId);
            return;
        }

        var (status, code, message) = MapException(ex);

        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";

        var error = new ApiErrorResponse
        {
            Message = message,
            ErrorCode = code,
            TraceId = traceId
        };

        await JsonSerializer.SerializeAsync(
            ctx.Response.Body,
            error,
            JsonOptions,
            ctx.RequestAborted);
    }

    private (int status, string code, string message) MapException(Exception ex)
    {
        return ex switch
        {
            // auth
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", "Unauthorized"),

            // validation
            ArgumentException arg =>
                (StatusCodes.Status400BadRequest, "BadRequest", arg.Message),

            // not found
            KeyNotFoundException nf =>
                (StatusCodes.Status404NotFound, "NotFound", nf.Message),

            // db conflict
            DbUpdateException =>
                (StatusCodes.Status409Conflict, "DbConflict", "Database conflict"),

            // timeout
            TimeoutException =>
                (StatusCodes.Status504GatewayTimeout, "Timeout", "Request timeout"),

            // default
            _ => (
                StatusCodes.Status500InternalServerError,
                "ServerError",
                _env.IsDevelopment() ? ex.Message : "Internal server error"
            )
        };
    }
}
