using System.Collections.ObjectModel;
using LibraryDesk.Application.Administration;
using LibraryDesk.Domain.Security;

namespace LibraryDesk.Desktop.ViewModels;

/// <summary>
/// Allows an administrator to provision staff accounts and make safe local backups.
/// </summary>
public sealed class AdministrationPageViewModel : ViewModelBase
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IDatabaseBackupService _databaseBackupService;
    private UserAccountDto? _selectedUser;
    private string _displayName = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private UserRole _selectedRole = UserRole.Librarian;
    private string? _statusMessage;

    public AdministrationPageViewModel(
        IUserAdministrationService userAdministrationService,
        IDatabaseBackupService databaseBackupService)
    {
        _userAdministrationService = userAdministrationService ?? throw new ArgumentNullException(nameof(userAdministrationService));
        _databaseBackupService = databaseBackupService ?? throw new ArgumentNullException(nameof(databaseBackupService));
        Users = new ObservableCollection<UserAccountDto>();
        AvailableRoles = Enum.GetValues<UserRole>();
        RefreshUsersCommand = new AsyncRelayCommand(RefreshUsersAsync);
        CreateUserCommand = new AsyncRelayCommand(CreateUserAsync);
        ToggleSelectedUserCommand = new AsyncRelayCommand(ToggleSelectedUserAsync, () => SelectedUser is not null);
        CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync);
        RefreshUsersCommand.Execute(null);
    }

    public ObservableCollection<UserAccountDto> Users { get; }

    public IReadOnlyList<UserRole> AvailableRoles { get; }

    public AsyncRelayCommand RefreshUsersCommand { get; }

    public AsyncRelayCommand CreateUserCommand { get; }

    public AsyncRelayCommand ToggleSelectedUserCommand { get; }

    public AsyncRelayCommand CreateBackupCommand { get; }

    public UserAccountDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                ToggleSelectedUserCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(ToggleSelectedUserLabel));
            }
        }
    }

    public string ToggleSelectedUserLabel => SelectedUser?.IsActive == false ? "Activate selected" : "Deactivate selected";

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public UserRole SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task RefreshUsersAsync()
    {
        var result = await _userAdministrationService.GetUsersAsync();
        if (!result.IsSuccess || result.Value is null)
        {
            StatusMessage = result.Error;
            return;
        }

        Users.Clear();
        foreach (var user in result.Value)
        {
            Users.Add(user);
        }
    }

    private async Task CreateUserAsync()
    {
        var result = await _userAdministrationService.CreateUserAsync(
            new CreateUserRequest(DisplayName, Username, Password, SelectedRole));
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error;
            return;
        }

        DisplayName = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        StatusMessage = $"Created {result.Value?.Role.ToString().ToLowerInvariant()} account '{result.Value?.Username}'.";
        await RefreshUsersAsync();
    }

    private async Task ToggleSelectedUserAsync()
    {
        if (SelectedUser is null)
        {
            return;
        }

        var result = await _userAdministrationService.SetUserActiveAsync(SelectedUser.Id, !SelectedUser.IsActive);
        StatusMessage = result.IsSuccess ? "User account updated." : result.Error;
        await RefreshUsersAsync();
    }

    private async Task CreateBackupAsync()
    {
        var destinationDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "LibraryDesk",
            "backups");
        var result = await _databaseBackupService.CreateBackupAsync(destinationDirectory);
        StatusMessage = result.IsSuccess
            ? $"Backup created: {result.Value?.FilePath} ({result.Value?.SizeInBytes:N0} bytes)."
            : result.Error;
    }
}
