namespace LibraryDesk.Domain.Books;

/// <summary>
/// Describes a physical copy's circulation availability.
/// </summary>
public enum CopyStatus
{
    Available = 0,
    OnLoan = 1,
    Withdrawn = 2,
    Lost = 3,
}
