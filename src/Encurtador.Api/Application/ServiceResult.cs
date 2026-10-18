namespace Encurtador.Api.Application;

public enum ErrorKind
{
    Validation,
    Conflict,
    NotFound,
    Gone,
}

public sealed record ServiceError(ErrorKind Kind, string Message);

public sealed record ServiceResult<T>(T? Value, ServiceError? Error)
{
    public bool IsSuccess => Error is null;

    public static ServiceResult<T> Success(T value) => new(value, null);

    public static ServiceResult<T> Failure(ErrorKind kind, string message) =>
        new(default, new ServiceError(kind, message));
}
