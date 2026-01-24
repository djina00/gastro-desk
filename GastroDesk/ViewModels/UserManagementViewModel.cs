using System.Collections.ObjectModel;
using System.Windows.Input;
using GastroDesk.Commands;
using GastroDesk.Data;
using GastroDesk.Models;
using GastroDesk.Models.Enums;
using GastroDesk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroDesk.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly DbContextFactory _dbContextFactory;

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<UserRole> AvailableRoles { get; } = new();

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        private string _newUsername = string.Empty;
        public string NewUsername
        {
            get => _newUsername;
            set
            {
                SetProperty(ref _newUsername, value);
                ClearError();
            }
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                SetProperty(ref _newPassword, value);
                ClearError();
            }
        }

        private string _newFirstName = string.Empty;
        public string NewFirstName
        {
            get => _newFirstName;
            set
            {
                SetProperty(ref _newFirstName, value);
                ClearError();
            }
        }

        private string _newLastName = string.Empty;
        public string NewLastName
        {
            get => _newLastName;
            set
            {
                SetProperty(ref _newLastName, value);
                ClearError();
            }
        }

        private UserRole _newUserRole = UserRole.Waiter;
        public UserRole NewUserRole
        {
            get => _newUserRole;
            set => SetProperty(ref _newUserRole, value);
        }

        private string? _successMessage;
        public string? SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public ICommand AddUserCommand { get; }
        public ICommand ToggleUserActiveCommand { get; }

        public UserManagementViewModel(IAuthService authService)
        {
            _authService = authService;
            _dbContextFactory = DbContextFactory.Instance;

            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                AvailableRoles.Add(role);
            }

            AddUserCommand = new AsyncRelayCommand(AddUserAsync, CanAddUser);
            ToggleUserActiveCommand = new AsyncRelayCommand(ToggleUserActiveAsync, () => SelectedUser != null);

            _ = LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                Users.Clear();

                using var context = _dbContextFactory.CreateContext();
                var users = await context.Users.OrderBy(u => u.Username).ToListAsync();

                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                SetError($"Failed to load users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanAddUser()
        {
            return !string.IsNullOrWhiteSpace(NewUsername) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   !string.IsNullOrWhiteSpace(NewFirstName) &&
                   !string.IsNullOrWhiteSpace(NewLastName);
        }

        private async Task AddUserAsync()
        {
            if (!CanAddUser())
            {
                SetError("Please fill in all fields.");
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();
                SuccessMessage = null;

                var newUser = new User
                {
                    Username = NewUsername.Trim(),
                    FirstName = NewFirstName.Trim(),
                    LastName = NewLastName.Trim(),
                    Role = NewUserRole,
                    IsActive = true
                };

                var success = await _authService.RegisterAsync(newUser, NewPassword);

                if (success)
                {
                    SuccessMessage = $"User '{NewUsername}' created successfully!";
                    ClearForm();
                    await LoadUsersAsync();
                }
                else
                {
                    SetError($"Username '{NewUsername}' already exists.");
                }
            }
            catch (Exception ex)
            {
                SetError($"Failed to create user: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleUserActiveAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                IsLoading = true;
                ClearError();
                SuccessMessage = null;

                using var context = _dbContextFactory.CreateContext();
                var user = await context.Users.FindAsync(SelectedUser.Id);

                if (user != null)
                {
                    user.IsActive = !user.IsActive;
                    user.UpdatedAt = DateTime.Now;
                    await context.SaveChangesAsync();

                    var status = user.IsActive ? "activated" : "deactivated";
                    SuccessMessage = $"User '{user.Username}' has been {status}.";
                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                SetError($"Failed to update user: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearForm()
        {
            NewUsername = string.Empty;
            NewPassword = string.Empty;
            NewFirstName = string.Empty;
            NewLastName = string.Empty;
            NewUserRole = UserRole.Waiter;
        }
    }
}
