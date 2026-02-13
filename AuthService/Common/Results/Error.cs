namespace AuthService.Common.Results;

public sealed class Error(ErrorCode code, string message)
{
    public ErrorCode Code { get; } = code;
    public string Message { get; } = message;

    public static Error None => new(ErrorCode.None, "");

    public static Error Validation(string msg)
        => new(ErrorCode.Validation, msg);

    public static Error NotFound(string msg)
        => new(ErrorCode.NotFound, msg);

    public static Error Conflict(string msg)
        => new(ErrorCode.Conflict, msg);
}