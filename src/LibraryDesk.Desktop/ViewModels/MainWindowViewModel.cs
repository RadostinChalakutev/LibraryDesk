using System.Collections.ObjectModel;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Domain.Security;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private NavigationItemViewModel? _selectedNavigationItem;
    private PlaceholderPageViewModel _currentPage;
    private readonly IUserSession _userSession;

    public MainWindowViewModel(IClock clock, IUserSession userSession)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));

        if (_userSession.CurrentUser is null)
        {
            throw new InvalidOperationException("A user must be authenticated before opening the main window.");
        }

        NavigationItems = new ObservableCollection<NavigationItemViewModel>();
        AddItemIfAllowed(Capability.ViewDashboard, "Dashboard", "A concise operational overview will be added in Phase 8.");
        AddItemIfAllowed(Capability.ManageReaders, "Readers", "Reader management will be added in Phase 4.");
        AddItemIfAllowed(Capability.ManageBooks, "Books", "Books and physical copies will be added in Phase 5.");
        AddItemIfAllowed(Capability.ManageLoans, "Loans", "Borrowing and returns will be added in Phase 6.");
        AddItemIfAllowed(Capability.ViewOverdue, "Overdue", "Overdue tracking will be added in Phase 7.");
        AddItemIfAllowed(Capability.ViewReports, "Reports", "Reports will be added in Phase 9.");
        AddItemIfAllowed(Capability.ManageSettings, "Settings", "Administration and backup will be added in Phase 10.");

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

    public PlaceholderPageViewModel CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    private static NavigationItemViewModel CreateItem(string title, string description) =>
        new(title, new PlaceholderPageViewModel(title, description));

    private void AddItemIfAllowed(Capability capability, string title, string description)
    {
        if (_userSession.HasPermission(capability))
        {
            NavigationItems.Add(CreateItem(title, description));
        }
    }

    private void SignOut()
    {
        _userSession.Clear();
        SignedOut?.Invoke(this, EventArgs.Empty);
    }
}
