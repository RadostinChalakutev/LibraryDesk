using System.Collections.ObjectModel;
using LibraryDesk.Application.Abstractions;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private NavigationItemViewModel? _selectedNavigationItem;
    private PlaceholderPageViewModel _currentPage;

    public MainWindowViewModel(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        NavigationItems = new ObservableCollection<NavigationItemViewModel>(
        [
            CreateItem("Dashboard", "A concise operational overview will be added in Phase 8."),
            CreateItem("Readers", "Reader management will be added in Phase 4."),
            CreateItem("Books", "Books and physical copies will be added in Phase 5."),
            CreateItem("Loans", "Borrowing and returns will be added in Phase 6."),
            CreateItem("Overdue", "Overdue tracking will be added in Phase 7."),
            CreateItem("Reports", "Reports will be added in Phase 9."),
            CreateItem("Settings", "Administration and backup will be added in Phase 10."),
        ]);

        _selectedNavigationItem = NavigationItems[0];
        _currentPage = _selectedNavigationItem.Page;
        StartedAtUtc = clock.UtcNow;
    }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public DateTimeOffset StartedAtUtc { get; }

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
}
