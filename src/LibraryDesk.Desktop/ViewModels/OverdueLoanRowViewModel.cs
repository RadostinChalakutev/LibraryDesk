using LibraryDesk.Application.Loans;

namespace LibraryDesk.Desktop.ViewModels;

/// <summary>
/// Presents an overdue loan with the calculated number of days late.
/// </summary>
public sealed class OverdueLoanRowViewModel
{
    public OverdueLoanRowViewModel(LoanDto loan, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(loan);
        MembershipNumber = loan.MembershipNumber;
        ReaderName = loan.ReaderName;
        BookTitle = loan.BookTitle;
        Barcode = loan.Barcode;
        DueOn = loan.DueOn;
        DaysOverdue = asOfDate.DayNumber - loan.DueOn.DayNumber;
    }

    public string MembershipNumber { get; }

    public string ReaderName { get; }

    public string BookTitle { get; }

    public string Barcode { get; }

    public DateOnly DueOn { get; }

    public int DaysOverdue { get; }
}
