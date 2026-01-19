using System.IO;
using Microsoft.EntityFrameworkCore;

namespace GastroDesk.Data
{
    /// <summary>
    /// Singleton factory for creating and managing database context instances.
    /// Implements the Singleton design pattern (Creational Pattern).
    /// </summary>
    public sealed class DbContextFactory
    {
        private static readonly Lazy<DbContextFactory> _instance =
            new Lazy<DbContextFactory>(() => new DbContextFactory());

        private readonly DbContextOptions<AppDbContext> _options;

        private DbContextFactory()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gastrodesk.db");
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
        }

        public static DbContextFactory Instance => _instance.Value;

        public AppDbContext CreateContext()
        {
            return new AppDbContext(_options);
        }

        public void EnsureDatabaseCreated()
        {
            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public void MigrateDatabase()
        {
            using var context = CreateContext();
            context.Database.Migrate();
        }
    }
}
