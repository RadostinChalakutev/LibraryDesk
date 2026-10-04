namespace LibraryDesk.Application.Primitives;

/// <summary>
/// Describes the outcome of an application operation without exposing transport concerns.
/// </summary>
public sealed record OperationResult
{
    private OperationResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public string? Error { get; }

    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new(false, error);
    }
}
