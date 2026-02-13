namespace AuthService.Contracts.Response;

public class ApiResponse<T>(bool success, string message, T? data)
{
    public bool Success { get; } = success;
    public string Message { get; } = message;
    public T? Data { get; } = data;

    public static ApiResponse<T> Ok(T data, string msg = "OK")
        => new(true, msg, data);

    public static ApiResponse<T> Fail(string msg)
        => new(false, msg, default);
}
