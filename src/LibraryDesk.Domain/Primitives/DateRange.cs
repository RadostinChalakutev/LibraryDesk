namespace LibraryDesk.Domain.Primitives;

/// <summary>
/// Represents an inclusive, validated range of calendar dates.
/// </summary>
public readonly record struct DateRange
{
    /// <summary>
    /// Creates an inclusive date range.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the end date precedes the start date.</exception>
    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new ArgumentException("The end date cannot precede the start date.", nameof(end));
        }

        Start = start;
        End = end;
    }

    public DateOnly Start { get; }

    public DateOnly End { get; }

    public int DaysInclusive => End.DayNumber - Start.DayNumber + 1;

    public bool Contains(DateOnly date) => date >= Start && date <= End;
}
