using System.Windows.Input;
using GastroDesk.Commands;
using GastroDesk.Models;
using GastroDesk.Models.Enums;
using GastroDesk.Services;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IMenuService _menuService;
        // private readonly IOrderService _orderService;
        // private readonly IReportService _reportService;

        private BaseViewModel? _currentViewModel;
        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                SetProperty(ref _currentUser, value);
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(IsManager));
                OnPropertyChanged(nameof(UserDisplayName));
            }
        }

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsManager => CurrentUser?.Role == UserRole.Manager;
        public string UserDisplayName => CurrentUser?.FullName ?? "";

        public ICommand NavigateToMenuCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }
        public ICommand NavigateToReportsCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            _authService = new AuthService();
            _menuService = new MenuService();
            // _orderService = new OrderService();
            // _reportService = new ReportService();

            NavigateToMenuCommand = new RelayCommand(() => NavigateToMenu(), () => IsLoggedIn);
            NavigateToOrdersCommand = new RelayCommand(() => NavigateToOrders(), () => IsLoggedIn);
            NavigateToReportsCommand = new RelayCommand(() => NavigateToReports(), () => IsLoggedIn && IsManager);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            ShowLogin();
        }

        private void ShowLogin()
        {
            var loginVm = new LoginViewModel(_authService);
            loginVm.LoginSuccessful += OnLoginSuccessful;
            CurrentViewModel = loginVm;
        }

        private void OnLoginSuccessful(object? sender, User user)
        {
            CurrentUser = user;
            NavigateToMenu();
        }

        private void NavigateToMenu()
        {
            CurrentViewModel = new MenuViewModel(_menuService, IsManager);
        }

        private void NavigateToOrders()
        {
            // CurrentViewModel = new OrderViewModel(_orderService, _menuService, CurrentUser!);
        }

        private void NavigateToReports()
        {
            // CurrentViewModel = new ReportViewModel(_reportService);
        }

        private void ExecuteLogout()
        {
            _authService.Logout();
            CurrentUser = null;
            ShowLogin();
        }
    }
}
