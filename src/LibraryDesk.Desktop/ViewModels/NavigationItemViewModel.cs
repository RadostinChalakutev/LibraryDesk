namespace LibraryDesk.Desktop.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string title, PlaceholderPageViewModel page)
    {
        Title = title;
        Page = page;
    }

    public string Title { get; }

    public PlaceholderPageViewModel Page { get; }
}
