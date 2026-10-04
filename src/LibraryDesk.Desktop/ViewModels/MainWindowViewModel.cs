using System.Collections.ObjectModel;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Domain.Security;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private NavigationItemViewModel? _selectedNavigationItem;
    private ViewModelBase _currentPage;
    private readonly IUserSession _userSession;

    public MainWindowViewModel(
        IClock clock,
        IUserSession userSession,
        DashboardPageViewModel dashboardPage,
        ReaderPageViewModel readerPage,
        BookPageViewModel bookPage,
        LoanPageViewModel loanPage,
        OverduePageViewModel overduePage,
        ReportsPageViewModel reportsPage,
        AdministrationPageViewModel administrationPage)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));

        if (_userSession.CurrentUser is null)
        {
            throw new InvalidOperationException("A user must be authenticated before opening the main window.");
        }

        NavigationItems = new ObservableCollection<NavigationItemViewModel>();
        AddItemIfAllowed(Capability.ViewDashboard, "Dashboard", dashboardPage);
        AddItemIfAllowed(Capability.ManageReaders, "Readers", readerPage);
        AddItemIfAllowed(Capability.ManageBooks, "Books", bookPage);
        AddItemIfAllowed(Capability.ManageLoans, "Loans", loanPage);
        AddItemIfAllowed(Capability.ViewOverdue, "Overdue", overduePage);
        AddItemIfAllowed(Capability.ViewReports, "Reports", reportsPage);
        AddItemIfAllowed(Capability.ManageSettings, "Settings", administrationPage);

        _selectedNavigationItem = NavigationItems[0];
        _currentPage = _selectedNavigationItem.Page;
        StartedAtUtc = clock.UtcNow;
        SignOutCommand = new RelayCommand(_ => SignOut());
    }

    public event EventHandler? SignedOut;

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public string CurrentUserDisplayName => _userSession.CurrentUser?.DisplayName ?? string.Empty;

    public string CurrentUserRole => _userSession.CurrentUser?.Role.ToString() ?? string.Empty;

    public RelayCommand SignOutCommand { get; }

    public NavigationItemViewModel? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetProperty(ref _selectedNavigationItem, value) && value is not null)
            {
                CurrentPage = value.Page;
            }
        }
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    private static NavigationItemViewModel CreateItem(string title, string description) =>
        new(title, new PlaceholderPageViewModel(title, description));

    private static NavigationItemViewModel CreateItem(string title, ViewModelBase page) => new(title, page);

    private void AddItemIfAllowed(Capability capability, string title, string description)
    {
        if (_userSession.HasPermission(capability))
        {
            NavigationItems.Add(CreateItem(title, description));
        }
    }

    private void AddItemIfAllowed(Capability capability, string title, ViewModelBase page)
    {
        if (_userSession.HasPermission(capability))
        {
            NavigationItems.Add(CreateItem(title, page));
        }
    }

    private void SignOut()
    {
        _userSession.Clear();
        SignedOut?.Invoke(this, EventArgs.Empty);
    }
}
