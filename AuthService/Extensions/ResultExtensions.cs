using AuthService.Common.Results;
using AuthService.Contracts.Response;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Extensions;

public static class ResultExtensions
{
    // =====================================================
    // GENERIC RESULT<T>
    // =====================================================
    public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext? ctx = null)
    {
        if (result.IsSuccess)
        {
            // SUCCESS → return raw data
            return new OkObjectResult(result.Value);
        }

        var traceId = ctx?.TraceIdentifier;
        var err = result.Error;

        var error = new ApiErrorResponse
        {
            Message = err.Message,
            ErrorCode = err.Code.ToString(),
            TraceId = traceId
        };

        return err.Code switch
        {
            ErrorCode.Validation =>
                new BadRequestObjectResult(error),

            ErrorCode.NotFound =>
                new NotFoundObjectResult(error),

            ErrorCode.Conflict =>
                new ConflictObjectResult(error),

            ErrorCode.Unauthorized or ErrorCode.InvalidCredential or ErrorCode.TokenExpired =>
                new UnauthorizedObjectResult(error),

            ErrorCode.Forbidden or ErrorCode.UserLocked =>
                new ObjectResult(error)
                { StatusCode = StatusCodes.Status403Forbidden },

            _ =>
                new ObjectResult(error)
                { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }

    // =====================================================
    // NON GENERIC RESULT
    // =====================================================
    public static IActionResult ToActionResult(this Result result, HttpContext? ctx = null)
    {
        if (result.IsSuccess)
        {
            return new OkResult();
        }

        var traceId = ctx?.TraceIdentifier;
        var err = result.Error;

        var error = new ApiErrorResponse
        {
            Message = err.Message,
            ErrorCode = err.Code.ToString(),
            TraceId = traceId
        };

        return err.Code switch
        {
            ErrorCode.Validation =>
                new BadRequestObjectResult(error),

            ErrorCode.NotFound =>
                new NotFoundObjectResult(error),

            ErrorCode.Conflict =>
                new ConflictObjectResult(error),

            ErrorCode.Unauthorized or ErrorCode.InvalidCredential or ErrorCode.TokenExpired =>
                new UnauthorizedObjectResult(error),

            ErrorCode.Forbidden or ErrorCode.UserLocked =>
                new ObjectResult(error)
                { StatusCode = StatusCodes.Status403Forbidden },

            _ =>
                new ObjectResult(error)
                { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }
}