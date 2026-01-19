using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using GastroDesk.Data;
using GastroDesk.Models;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.Services
{
    public class AuthService : IAuthService
    {
        private readonly DbContextFactory _dbContextFactory;

        public User? CurrentUser { get; private set; }

        public AuthService()
        {
            _dbContextFactory = DbContextFactory.Instance;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            using var context = _dbContextFactory.CreateContext();
            var passwordHash = HashPassword(password);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username &&
                                          u.PasswordHash == passwordHash &&
                                          u.IsActive);

            if (user != null)
            {
                CurrentUser = user;
            }

            return user;
        }

        public async Task<bool> RegisterAsync(User user, string password)
        {
            using var context = _dbContextFactory.CreateContext();

            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username);

            if (existingUser != null)
            {
                return false;
            }

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.Now;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            using var context = _dbContextFactory.CreateContext();
            var user = await context.Users.FindAsync(userId);

            if (user == null || user.PasswordHash != HashPassword(oldPassword))
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            return true;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
