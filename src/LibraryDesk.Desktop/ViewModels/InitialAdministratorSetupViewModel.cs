using LibraryDesk.Application.Authentication;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class InitialAdministratorSetupViewModel : ViewModelBase
{
    private readonly IInitialAdministratorSetupService _setupService;
    private readonly IUserSession _userSession;
    private string _displayName = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string? _errorMessage;
    private bool _isBusy;

    public InitialAdministratorSetupViewModel(
        IInitialAdministratorSetupService setupService,
        IUserSession userSession)
    {
        _setupService = setupService ?? throw new ArgumentNullException(nameof(setupService));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync, () => IsNotBusy);
    }

    public event EventHandler? AdministratorCreated;

    public AsyncRelayCommand CreateAccountCommand { get; }

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

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                CreateAccountCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    private async Task CreateAccountAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                ErrorMessage = "Password confirmation does not match.";
                return;
            }

            var result = await _setupService.CreateInitialAdministratorAsync(
                new InitialAdministratorRequest(DisplayName, Username, Password));
            if (!result.IsSuccess || result.User is null)
            {
                ErrorMessage = result.ErrorMessage ?? "Unable to create the administrator account.";
                return;
            }

            _userSession.Start(result.User);
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            AdministratorCreated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to create the administrator account. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
