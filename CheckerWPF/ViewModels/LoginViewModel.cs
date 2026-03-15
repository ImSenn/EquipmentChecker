using CheckerWPF.Services;

namespace CheckerWPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _auth;
        private readonly SignalRService _signalR;
        private readonly Action _onLoginSuccess;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        public AsyncRelayCommand LoginCommand { get; }

        public LoginViewModel(AuthService auth, SignalRService signalR, Action onLoginSuccess)
        {
            _auth = auth;
            _signalR = signalR;
            _onLoginSuccess = onLoginSuccess;

            LoginCommand = new AsyncRelayCommand(DoLoginAsync,
                () => !IsLoading && !string.IsNullOrWhiteSpace(Username));
        }

        private async Task DoLoginAsync()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                var ok = await _auth.LoginAsync(Username, Password);
                if (!ok)
                {
                    ErrorMessage = "Sai username hoặc password.";
                    return;
                }

                // Kết nối SignalR ngay sau login
                await _signalR.ConnectAsync(_auth.Token!);
                _onLoginSuccess();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
