using System.Collections.ObjectModel;
using LibraryDesk.Application.Readers;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class ReaderPageViewModel : ViewModelBase
{
    private readonly IReaderService _readerService;
    private ReaderDto? _selectedReader;
    private string _searchText = string.Empty;
    private bool _includeInactive;
    private string _membershipNumber = string.Empty;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string? _statusMessage;

    public ReaderPageViewModel(IReaderService readerService)
    {
        _readerService = readerService ?? throw new ArgumentNullException(nameof(readerService));
        Readers = new ObservableCollection<ReaderDto>();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ToggleActiveCommand = new AsyncRelayCommand(ToggleActiveAsync, () => SelectedReader is not null);
        NewReaderCommand = new RelayCommand(_ => ClearForm());
        RefreshCommand.Execute(null);
    }

    public ObservableCollection<ReaderDto> Readers { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand ToggleActiveCommand { get; }

    public RelayCommand NewReaderCommand { get; }

    public ReaderDto? SelectedReader
    {
        get => _selectedReader;
        set
        {
            if (SetProperty(ref _selectedReader, value))
            {
                ToggleActiveCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(IsEditingExistingReader));
                OnPropertyChanged(nameof(CanEditMembership));
                if (value is not null)
                {
                    MembershipNumber = value.MembershipNumber;
                    FullName = value.FullName;
                    Email = value.Email ?? string.Empty;
                    Phone = value.Phone ?? string.Empty;
                    StatusMessage = null;
                }
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool IncludeInactive
    {
        get => _includeInactive;
        set => SetProperty(ref _includeInactive, value);
    }

    public string MembershipNumber
    {
        get => _membershipNumber;
        set => SetProperty(ref _membershipNumber, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsEditingExistingReader => SelectedReader is not null;

    public bool CanEditMembership => !IsEditingExistingReader;

    private async Task RefreshAsync()
    {
        var results = await _readerService.SearchAsync(SearchText, IncludeInactive);
        Readers.Clear();
        foreach (var reader in results)
        {
            Readers.Add(reader);
        }
    }

    private async Task SaveAsync()
    {
        if (SelectedReader is null)
        {
            var createResult = await _readerService.CreateAsync(
                new CreateReaderRequest(MembershipNumber, FullName, Email, Phone));
            if (!createResult.IsSuccess)
            {
                StatusMessage = createResult.Error;
                return;
            }

            StatusMessage = "Reader registered.";
            ClearForm();
        }
        else
        {
            var updateResult = await _readerService.UpdateAsync(
                new UpdateReaderRequest(SelectedReader.Id, FullName, Email, Phone));
            if (!updateResult.IsSuccess)
            {
                StatusMessage = updateResult.Error;
                return;
            }

            StatusMessage = "Reader updated.";
        }

        await RefreshAsync();
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedReader is null)
        {
            return;
        }

        var result = await _readerService.SetActiveAsync(SelectedReader.Id, !SelectedReader.IsActive);
        StatusMessage = result.IsSuccess
            ? SelectedReader.IsActive ? "Reader deactivated." : "Reader reactivated."
            : result.Error;
        await RefreshAsync();
    }

    private void ClearForm()
    {
        SelectedReader = null;
        MembershipNumber = string.Empty;
        FullName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        StatusMessage = null;
    }
}
