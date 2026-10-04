using LibraryDesk.Domain.Primitives;

namespace LibraryDesk.Domain.Tests.Primitives;

public sealed class DateRangeTests
{
    [Fact]
    public void CreatesInclusiveRangeAndContainsBoundaryDates()
    {
        var range = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 4));

        Assert.Equal(4, range.DaysInclusive);
        Assert.True(range.Contains(new DateOnly(2026, 10, 1)));
        Assert.True(range.Contains(new DateOnly(2026, 10, 4)));
        Assert.False(range.Contains(new DateOnly(2026, 10, 5)));
    }

    [Fact]
    public void RejectsReverseRange()
    {
        Assert.Throws<ArgumentException>(() =>
            new DateRange(new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 1)));
    }
}
