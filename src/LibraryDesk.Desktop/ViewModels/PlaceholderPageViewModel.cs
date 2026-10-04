namespace LibraryDesk.Desktop.ViewModels;

/// <summary>
/// Represents a deliberately empty feature area that will be implemented in a later phase.
/// </summary>
public sealed class PlaceholderPageViewModel : ViewModelBase
{
    public PlaceholderPageViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }
}
