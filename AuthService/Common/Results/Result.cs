namespace AuthService.Common.Results;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool success, Error error)
    {
        IsSuccess = success;
        Error = error;
    }

    public static Result Success()
        => new(true, Error.None);

    public static Result Failure(Error error)
        => new(false, error);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool success, T? value, Error error)
        : base(success, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
        => new(true, value, Error.None);

    public new static Result<T> Failure(Error error)
        => new(false, default, error);
}
