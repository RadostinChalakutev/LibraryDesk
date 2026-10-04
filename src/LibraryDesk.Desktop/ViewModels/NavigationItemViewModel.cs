namespace LibraryDesk.Desktop.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string title, ViewModelBase page)
    {
        Title = title;
        Page = page;
    }

    public string Title { get; }

    public ViewModelBase Page { get; }
}
