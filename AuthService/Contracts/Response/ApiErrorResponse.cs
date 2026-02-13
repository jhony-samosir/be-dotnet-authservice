namespace AuthService.Contracts.Response;

public sealed class ApiErrorResponse
{
    public string Message { get; init; } = "";
    public string ErrorCode { get; init; } = "";
    public string? TraceId { get; init; }
}
