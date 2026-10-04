namespace LibraryDesk.Application.Abstractions;

/// <summary>
/// Supplies the current time while keeping application behavior testable.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
