using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LibraryDesk.Desktop.Views;

public partial class BookPageView : UserControl
{
    public BookPageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
