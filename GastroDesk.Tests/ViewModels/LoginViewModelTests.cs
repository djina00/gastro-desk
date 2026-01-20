using GastroDesk.Models;
using GastroDesk.Models.Enums;
using GastroDesk.Services.Interfaces;
using GastroDesk.ViewModels;

namespace GastroDesk.Tests.ViewModels;

public class LoginViewModelTests
{
    private class FakeAuthService : IAuthService
    {
        public User? CurrentUser { get; private set; }
        public bool ShouldSucceed { get; set; } = true;

        public Task<User?> LoginAsync(string username, string password)
        {
            if (ShouldSucceed && username == "admin" && password == "admin123")
            {
                var user = new User
                {
                    Id = 1,
                    Username = username,
                    FirstName = "Admin",
                    LastName = "User",
                    Role = UserRole.Manager
                };
                CurrentUser = user;
                return Task.FromResult<User?>(user);
            }
            return Task.FromResult<User?>(null);
        }

        public Task<bool> RegisterAsync(User user, string password) => Task.FromResult(true);
        public Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword) => Task.FromResult(true);
        public string HashPassword(string password) => password;
        public void Logout() => CurrentUser = null;
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var authService = new FakeAuthService();
        var viewModel = new LoginViewModel(authService);
        viewModel.Username = "admin";
        viewModel.Password = "admin123";
        User? loggedInUser = null;
        viewModel.LoginSuccessful += (s, u) => loggedInUser = u;

        // Act
        viewModel.LoginCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        Assert.NotNull(loggedInUser);
        Assert.Equal("admin", loggedInUser.Username);
        Assert.Null(viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_SetsErrorMessage()
    {
        // Arrange
        var authService = new FakeAuthService();
        var viewModel = new LoginViewModel(authService);
        viewModel.Username = "wrong";
        viewModel.Password = "wrong";
        User? loggedInUser = null;
        viewModel.LoginSuccessful += (s, u) => loggedInUser = u;

        // Act
        viewModel.LoginCommand.Execute(null);
        await Task.Delay(100);

        // Assert
        Assert.Null(loggedInUser);
        Assert.NotNull(viewModel.ErrorMessage);
    }

    [Fact]
    public void Username_WhenSet_ClearsErrorMessage()
    {
        // Arrange
        var authService = new FakeAuthService();
        var viewModel = new LoginViewModel(authService);
        viewModel.ErrorMessage = "Some error";

        // Act
        viewModel.Username = "newuser";

        // Assert
        Assert.Null(viewModel.ErrorMessage);
    }
}
