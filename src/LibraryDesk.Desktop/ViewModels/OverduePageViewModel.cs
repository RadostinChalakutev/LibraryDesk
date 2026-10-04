using System.Collections.ObjectModel;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Loans;

namespace LibraryDesk.Desktop.ViewModels;

/// <summary>
/// Provides the actionable list of currently overdue loans.
/// </summary>
public sealed class OverduePageViewModel : ViewModelBase
{
    private readonly ILoanService _loanService;
    private readonly IClock _clock;
    private string? _statusMessage;

    public OverduePageViewModel(ILoanService loanService, IClock clock)
    {
        _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        OverdueLoans = new ObservableCollection<OverdueLoanRowViewModel>();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        RefreshCommand.Execute(null);
    }

    public ObservableCollection<OverdueLoanRowViewModel> OverdueLoans { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task RefreshAsync()
    {
        var asOfDate = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var loans = await _loanService.GetOverdueAsync(asOfDate);
        OverdueLoans.Clear();
        foreach (var loan in loans)
        {
            OverdueLoans.Add(new OverdueLoanRowViewModel(loan, asOfDate));
        }

        StatusMessage = OverdueLoans.Count == 0
            ? $"No overdue loans as of {asOfDate:yyyy-MM-dd}."
            : $"{OverdueLoans.Count} overdue loan(s) as of {asOfDate:yyyy-MM-dd}.";
    }
}
