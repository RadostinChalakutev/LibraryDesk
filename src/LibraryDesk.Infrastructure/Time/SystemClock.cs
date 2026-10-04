using LibraryDesk.Application.Abstractions;

namespace LibraryDesk.Infrastructure.Time;

/// <summary>
/// Production implementation of the application clock.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
