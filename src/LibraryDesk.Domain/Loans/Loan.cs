namespace LibraryDesk.Domain.Loans;

/// <summary>
/// Records the circulation of one physical copy to one reader.
/// </summary>
public sealed class Loan
{
    private Loan()
    {
    }

    private Loan(Guid id, Guid readerId, Guid physicalCopyId, DateOnly borrowedOn, DateOnly dueOn, DateTimeOffset createdAtUtc)
    {
        Id = id;
        ReaderId = readerId;
        PhysicalCopyId = physicalCopyId;
        BorrowedOn = borrowedOn;
        DueOn = dueOn;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ReaderId { get; private set; }

    public Guid PhysicalCopyId { get; private set; }

    public DateOnly BorrowedOn { get; private set; }

    public DateOnly DueOn { get; private set; }

    public DateOnly? ReturnedOn { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Loan Create(Guid readerId, Guid physicalCopyId, DateOnly borrowedOn, DateOnly dueOn, DateTimeOffset createdAtUtc)
    {
        if (readerId == Guid.Empty || physicalCopyId == Guid.Empty)
        {
            throw new ArgumentException("Reader and physical copy are required.");
        }

        if (dueOn <= borrowedOn)
        {
            throw new ArgumentException("Due date must be after the borrowing date.", nameof(dueOn));
        }

        return new Loan(Guid.NewGuid(), readerId, physicalCopyId, borrowedOn, dueOn, createdAtUtc);
    }

    public void Return(DateOnly returnedOn)
    {
        if (ReturnedOn is not null)
        {
            throw new InvalidOperationException("This loan has already been returned.");
        }

        if (returnedOn < BorrowedOn)
        {
            throw new ArgumentException("Return date cannot precede the borrowing date.", nameof(returnedOn));
        }

        ReturnedOn = returnedOn;
    }
}
