using System.Collections.ObjectModel;
using System.Globalization;
using LibraryDesk.Application.Loans;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class LoanPageViewModel : ViewModelBase
{
    private readonly ILoanService _loanService;
    private LoanDto? _selectedLoan;
    private string _membershipNumber = string.Empty;
    private string _barcode = string.Empty;
    private string _dueOn = DateTime.UtcNow.Date.AddDays(14).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    private string? _statusMessage;

    public LoanPageViewModel(ILoanService loanService)
    {
        _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
        ActiveLoans = new ObservableCollection<LoanDto>();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        BorrowCommand = new AsyncRelayCommand(BorrowAsync);
        ReturnCommand = new AsyncRelayCommand(ReturnAsync, () => SelectedLoan is not null);
        RefreshCommand.Execute(null);
    }

    public ObservableCollection<LoanDto> ActiveLoans { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand BorrowCommand { get; }

    public AsyncRelayCommand ReturnCommand { get; }

    public LoanDto? SelectedLoan
    {
        get => _selectedLoan;
        set
        {
            if (SetProperty(ref _selectedLoan, value))
            {
                ReturnCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string MembershipNumber
    {
        get => _membershipNumber;
        set => SetProperty(ref _membershipNumber, value);
    }

    public string Barcode
    {
        get => _barcode;
        set => SetProperty(ref _barcode, value);
    }

    public string DueOn
    {
        get => _dueOn;
        set => SetProperty(ref _dueOn, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task RefreshAsync()
    {
        var loans = await _loanService.GetActiveAsync();
        ActiveLoans.Clear();
        foreach (var loan in loans)
        {
            ActiveLoans.Add(loan);
        }
    }

    private async Task BorrowAsync()
    {
        if (!DateOnly.TryParseExact(DueOn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueOn))
        {
            StatusMessage = "Due date must use YYYY-MM-DD.";
            return;
        }

        var result = await _loanService.BorrowAsync(new BorrowRequest(MembershipNumber, Barcode, dueOn));
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error;
            return;
        }

        Barcode = string.Empty;
        StatusMessage = $"Borrowed until {result.Value?.DueOn:yyyy-MM-dd}.";
        await RefreshAsync();
    }

    private async Task ReturnAsync()
    {
        if (SelectedLoan is null)
        {
            return;
        }

        var result = await _loanService.ReturnAsync(SelectedLoan.Id);
        StatusMessage = result.IsSuccess ? "Copy returned to the library." : result.Error;
        await RefreshAsync();
    }
}
