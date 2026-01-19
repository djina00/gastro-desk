using System.IO;
using Microsoft.EntityFrameworkCore;
using GastroDesk.Models;

namespace GastroDesk.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gastrodesk.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasMany(c => c.Dishes)
                      .WithOne(d => d.Category)
                      .HasForeignKey(d => d.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Dish configuration
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.HasMany(d => d.OrderItems)
                      .WithOne(oi => oi.Dish)
                      .HasForeignKey(oi => oi.DishId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default manager user (password: admin123)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", // SHA256 of "admin123"
                    FirstName = "Admin",
                    LastName = "User",
                    Role = Models.Enums.UserRole.Manager,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new User
                {
                    Id = 2,
                    Username = "waiter1",
                    PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", // SHA256 of "admin123"
                    FirstName = "John",
                    LastName = "Doe",
                    Role = Models.Enums.UserRole.Waiter,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Seed categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Appetizers", Description = "Starters and small plates", CreatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 2, Name = "Main Courses", Description = "Main dishes", CreatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 3, Name = "Desserts", Description = "Sweet treats", CreatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 4, Name = "Beverages", Description = "Drinks and refreshments", CreatedAt = new DateTime(2024, 1, 1) }
            );

            // Seed dishes
            modelBuilder.Entity<Dish>().HasData(
                new Dish { Id = 1, Name = "Caesar Salad", Description = "Fresh romaine lettuce with Caesar dressing", Price = 8.99m, CategoryId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 2, Name = "Garlic Bread", Description = "Toasted bread with garlic butter", Price = 4.99m, CategoryId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 3, Name = "Grilled Steak", Description = "Premium beef steak with vegetables", Price = 24.99m, CategoryId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 4, Name = "Pasta Carbonara", Description = "Creamy pasta with bacon", Price = 14.99m, CategoryId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 5, Name = "Grilled Salmon", Description = "Fresh salmon with lemon sauce", Price = 19.99m, CategoryId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 6, Name = "Chocolate Cake", Description = "Rich chocolate layer cake", Price = 6.99m, CategoryId = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 7, Name = "Ice Cream", Description = "Three scoops of premium ice cream", Price = 5.99m, CategoryId = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 8, Name = "Cola", Description = "Refreshing cola drink", Price = 2.99m, CategoryId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 9, Name = "Fresh Orange Juice", Description = "Freshly squeezed orange juice", Price = 4.49m, CategoryId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Dish { Id = 10, Name = "Coffee", Description = "Hot brewed coffee", Price = 2.49m, CategoryId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
            );
        }
    }
}
