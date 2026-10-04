namespace LibraryDesk.Application.Primitives;

/// <summary>
/// Describes a successful operation that returns a value or an error outcome.
/// </summary>
public sealed record OperationResult<T>
{
    public OperationResult(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public string? Error { get; }

}

/// <summary>
/// Creates typed operation results without placing static members on generic result types.
/// </summary>
public static class OperationResults
{
    public static OperationResult<T> Success<T>(T value) => new(true, value, null);

    public static OperationResult<T> Failure<T>(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new(false, default, error);
    }
}
