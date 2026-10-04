using LibraryDesk.Infrastructure.Time;

namespace LibraryDesk.Infrastructure.Tests.Time;

public sealed class SystemClockTests
{
    [Fact]
    public void UtcNowUsesUtcOffset()
    {
        var clock = new SystemClock();

        Assert.Equal(TimeSpan.Zero, clock.UtcNow.Offset);
    }
}
