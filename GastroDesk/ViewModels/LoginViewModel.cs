using System.Windows.Input;
using GastroDesk.Commands;
using GastroDesk.Models;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                ClearError();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ClearError();
            }
        }

        public ICommand LoginCommand { get; }

        public event EventHandler<User>? LoginSuccessful;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, () => CanLogin());
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private async Task ExecuteLoginAsync()
        {
            if (!CanLogin())
                return;

            IsLoading = true;
            ClearError();

            try
            {
                var user = await _authService.LoginAsync(Username, Password);

                if (user != null)
                {
                    LoginSuccessful?.Invoke(this, user);
                }
                else
                {
                    SetError("Invalid username or password");
                }
            }
            catch (Exception ex)
            {
                SetError($"Login error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
