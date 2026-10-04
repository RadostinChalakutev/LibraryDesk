namespace LibraryDesk.Domain.Books;

/// <summary>
/// A barcode-addressable physical copy of a book title.
/// </summary>
public sealed class PhysicalCopy
{
    private PhysicalCopy()
    {
        Barcode = string.Empty;
    }

    private PhysicalCopy(Guid id, Guid bookId, string barcode, DateTimeOffset createdAtUtc)
    {
        Id = id;
        BookId = bookId;
        Barcode = NormalizeBarcode(barcode);
        Status = CopyStatus.Available;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid BookId { get; private set; }

    public string Barcode { get; private set; }

    public CopyStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static PhysicalCopy Create(Guid bookId, string barcode, DateTimeOffset createdAtUtc)
    {
        if (bookId == Guid.Empty)
        {
            throw new ArgumentException("Book is required.", nameof(bookId));
        }

        return new PhysicalCopy(Guid.NewGuid(), bookId, barcode, createdAtUtc);
    }

    public void StartLoan()
    {
        if (Status != CopyStatus.Available)
        {
            throw new InvalidOperationException("Only an available copy can be borrowed.");
        }

        Status = CopyStatus.OnLoan;
    }

    public void ReturnToLibrary()
    {
        if (Status != CopyStatus.OnLoan)
        {
            throw new InvalidOperationException("Only a borrowed copy can be returned.");
        }

        Status = CopyStatus.Available;
    }

    public void SetStatus(CopyStatus status)
    {
        if (Status == CopyStatus.OnLoan && status is CopyStatus.Withdrawn or CopyStatus.Lost)
        {
            throw new InvalidOperationException("Return a borrowed copy before changing it to lost or withdrawn.");
        }

        Status = status;
    }

    private static string NormalizeBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            throw new ArgumentException("Barcode is required.", nameof(barcode));
        }

        var normalized = barcode.Trim().ToUpperInvariant();
        if (normalized.Length > 64)
        {
            throw new ArgumentException("Barcode must not exceed 64 characters.", nameof(barcode));
        }

        return normalized;
    }
}
