using LibraryDesk.Application.Authentication;

namespace LibraryDesk.Desktop.ViewModels;

public sealed class LoginWindowViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserSession _userSession;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string? _errorMessage;
    private bool _isBusy;

    public LoginWindowViewModel(
        IAuthenticationService authenticationService,
        IUserSession userSession)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        SignInCommand = new AsyncRelayCommand(SignInAsync, () => IsNotBusy);
    }

    public event EventHandler? SignedIn;

    public AsyncRelayCommand SignInCommand { get; }

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
                SignInCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    private async Task SignInAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _authenticationService.AuthenticateAsync(Username, Password);
            if (!result.IsSuccess || result.User is null)
            {
                ErrorMessage = result.ErrorMessage ?? "Unable to sign in.";
                return;
            }

            _userSession.Start(result.User);
            Password = string.Empty;
            SignedIn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to sign in. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
