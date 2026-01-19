using GastroDesk.Models;

namespace GastroDesk.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(User user, string password);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        string HashPassword(string password);
        User? CurrentUser { get; }
        void Logout();
    }
}
