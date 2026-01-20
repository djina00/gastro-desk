using GastroDesk.Services;

namespace GastroDesk.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public void HashPassword_SameInput_ReturnsSameHash()
    {
        // Arrange
        var authService = new AuthService();
        var password = "testpassword123";

        // Act
        var hash1 = authService.HashPassword(password);
        var hash2 = authService.HashPassword(password);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_DifferentInputs_ReturnsDifferentHashes()
    {
        // Arrange
        var authService = new AuthService();

        // Act
        var hash1 = authService.HashPassword("password1");
        var hash2 = authService.HashPassword("password2");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_ReturnsValidSha256Hash()
    {
        // Arrange
        var authService = new AuthService();
        var password = "admin123";

        // Act
        var hash = authService.HashPassword(password);

        // Assert
        Assert.Equal(64, hash.Length); // SHA256 produces 64 hex characters
        Assert.Matches("^[a-f0-9]+$", hash); // Only lowercase hex characters
    }

    [Fact]
    public void Logout_ClearsCurrentUser()
    {
        // Arrange
        var authService = new AuthService();

        // Act
        authService.Logout();

        // Assert
        Assert.Null(authService.CurrentUser);
    }
}
