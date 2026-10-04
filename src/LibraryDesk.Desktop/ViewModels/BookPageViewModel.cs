using System.Collections.ObjectModel;
using LibraryDesk.Application.Books;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class BookPageViewModel : ViewModelBase
{
    private readonly IBookService _bookService;
    private BookDto? _selectedBook;
    private string _searchText = string.Empty;
    private string _title = string.Empty;
    private string _author = string.Empty;
    private string _isbn = string.Empty;
    private string _category = string.Empty;
    private string _publishedYear = string.Empty;
    private string _barcode = string.Empty;
    private string? _statusMessage;

    public BookPageViewModel(IBookService bookService)
    {
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        Books = new ObservableCollection<BookDto>();
        Copies = new ObservableCollection<CopyDto>();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        AddBookCommand = new AsyncRelayCommand(AddBookAsync);
        AddCopyCommand = new AsyncRelayCommand(AddCopyAsync, () => SelectedBook is not null);
        GenerateBarcodeCommand = new AsyncRelayCommand(GenerateBarcodeAsync, () => SelectedBook is not null);
        NewBookCommand = new RelayCommand(_ => ClearBookForm());
        RefreshCommand.Execute(null);
    }

    public ObservableCollection<BookDto> Books { get; }

    public ObservableCollection<CopyDto> Copies { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand AddBookCommand { get; }

    public AsyncRelayCommand AddCopyCommand { get; }

    public AsyncRelayCommand GenerateBarcodeCommand { get; }

    public RelayCommand NewBookCommand { get; }

    public BookDto? SelectedBook
    {
        get => _selectedBook;
        set
        {
            if (SetProperty(ref _selectedBook, value))
            {
                AddCopyCommand.NotifyCanExecuteChanged();
                GenerateBarcodeCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(SelectedBookTitle));
                _ = LoadCopiesAsync();
            }
        }
    }

    public string SelectedBookTitle => SelectedBook?.Title ?? "Select a book to add a copy";

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Author
    {
        get => _author;
        set => SetProperty(ref _author, value);
    }

    public string Isbn
    {
        get => _isbn;
        set => SetProperty(ref _isbn, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string PublishedYear
    {
        get => _publishedYear;
        set => SetProperty(ref _publishedYear, value);
    }

    public string Barcode
    {
        get => _barcode;
        set => SetProperty(ref _barcode, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task RefreshAsync()
    {
        var books = await _bookService.SearchAsync(SearchText);
        Books.Clear();
        foreach (var book in books)
        {
            Books.Add(book);
        }
    }

    private async Task AddBookAsync()
    {
        int? publishedYear = null;
        if (!string.IsNullOrWhiteSpace(PublishedYear))
        {
            if (!int.TryParse(PublishedYear, out var parsedYear))
            {
                StatusMessage = "Published year must be numeric.";
                return;
            }

            publishedYear = parsedYear;
        }

        var result = await _bookService.CreateAsync(new CreateBookRequest(Title, Author, Isbn, Category, publishedYear));
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error;
            return;
        }

        StatusMessage = "Book added to the catalogue.";
        ClearBookForm();
        await RefreshAsync();
    }

    private async Task AddCopyAsync()
    {
        if (SelectedBook is null)
        {
            return;
        }

        var result = await _bookService.AddCopyAsync(new AddCopyRequest(SelectedBook.Id, Barcode));
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error;
            return;
        }

        Barcode = string.Empty;
        StatusMessage = "Physical copy added.";
        await LoadCopiesAsync();
        await RefreshAsync();
    }

    private async Task GenerateBarcodeAsync()
    {
        if (SelectedBook is null)
        {
            return;
        }

        var result = await _bookService.GenerateNextBarcodeAsync(SelectedBook.Id);
        if (!result.IsSuccess || result.Value is null)
        {
            StatusMessage = result.Error;
            return;
        }

        Barcode = result.Value;
        StatusMessage = "Barcode generated. Add the physical copy to make it available for borrowing.";
    }

    private async Task LoadCopiesAsync()
    {
        Copies.Clear();
        if (SelectedBook is null)
        {
            return;
        }

        var copies = await _bookService.GetCopiesAsync(SelectedBook.Id);
        foreach (var copy in copies)
        {
            Copies.Add(copy);
        }
    }

    private void ClearBookForm()
    {
        Title = string.Empty;
        Author = string.Empty;
        Isbn = string.Empty;
        Category = string.Empty;
        PublishedYear = string.Empty;
    }
}
