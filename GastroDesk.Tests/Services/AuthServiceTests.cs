using GastroDesk.Services;

namespace GastroDesk.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService();
    }

    [Fact]
    public void HashPassword_SamePassword_ProducesSameHash()
    {
        // Arrange
        var password = "MySecurePassword123";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_DifferentPasswords_ProduceDifferentHashes()
    {
        // Arrange & Act
        var hash1 = _authService.HashPassword("Password123");
        var hash2 = _authService.HashPassword("Password456");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Logout_SetsCurrentUserToNull()
    {
        // Act
        _authService.Logout();

        // Assert
        Assert.Null(_authService.CurrentUser);
    }
}
