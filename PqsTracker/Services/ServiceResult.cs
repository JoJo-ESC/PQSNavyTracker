namespace PqsTracker.Services;

public enum ServiceErrorType
{
    None,
    NotFound,
    Validation
}

// Non-generic form for operations that don't return a value (e.g. delete).
public class ServiceResult
{
    public ServiceErrorType ErrorType { get; init; } = ServiceErrorType.None;
    public string? Error { get; init; }
    public bool IsSuccess => ErrorType == ServiceErrorType.None;

    public static ServiceResult Success() => new();
    public static ServiceResult NotFound(string message) => new() { ErrorType = ServiceErrorType.NotFound, Error = message };
    public static ServiceResult Invalid(string message) => new() { ErrorType = ServiceErrorType.Validation, Error = message };
}

// Generic form for operations that return a value on success.
public class ServiceResult<T> : ServiceResult
{
    public T? Value { get; init; }

    public static ServiceResult<T> Success(T value) => new() { Value = value };
    public new static ServiceResult<T> NotFound(string message) => new() { ErrorType = ServiceErrorType.NotFound, Error = message };
    public new static ServiceResult<T> Invalid(string message) => new() { ErrorType = ServiceErrorType.Validation, Error = message };
}
