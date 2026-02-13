using AuthService.Common.Results;
using AuthService.Contracts.Response;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(
                ApiResponse<T>.Ok(result.Value!)
            );
        }

        return result.Error.Code switch
        {
            ErrorCode.Validation =>
                new BadRequestObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.NotFound =>
                new NotFoundObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.Conflict =>
                new ConflictObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.Unauthorized or ErrorCode.InvalidCredential or ErrorCode.TokenExpired =>
                new UnauthorizedObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.Forbidden or ErrorCode.UserLocked =>
                new ObjectResult(ApiResponse<string>.Fail(result.Error.Message))
                { StatusCode = StatusCodes.Status403Forbidden },

            _ => new ObjectResult(ApiResponse<string>.Fail(result.Error.Message))
                { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }

    // non generic result
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse<string>.Ok("OK"));

        return result.Error.Code switch
        {
            ErrorCode.Validation =>
                new BadRequestObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.NotFound =>
                new NotFoundObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.Conflict =>
                new ConflictObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.Unauthorized or ErrorCode.InvalidCredential or ErrorCode.TokenExpired =>
                new UnauthorizedObjectResult(ApiResponse<string>.Fail(result.Error.Message)),

            ErrorCode.Forbidden or ErrorCode.UserLocked =>
                new ObjectResult(ApiResponse<string>.Fail(result.Error.Message))
                { StatusCode = StatusCodes.Status403Forbidden },

            _ => new ObjectResult(ApiResponse<string>.Fail(result.Error.Message))
            { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }
}