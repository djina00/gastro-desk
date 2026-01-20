using Microsoft.EntityFrameworkCore;
using GastroDesk.Data;
using GastroDesk.Models;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.Services
{
    public class MenuService : IMenuService
    {
        private readonly DbContextFactory _dbContextFactory;

        public MenuService()
        {
            _dbContextFactory = DbContextFactory.Instance;
        }

        #region Categories

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Categories
                .Include(c => c.Dishes)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            using var context = _dbContextFactory.CreateContext();
            category.CreatedAt = DateTime.Now;
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            using var context = _dbContextFactory.CreateContext();
            var existing = await context.Categories.FindAsync(category.Id);

            if (existing == null)
                throw new InvalidOperationException("Category not found");

            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using var context = _dbContextFactory.CreateContext();
            var category = await context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            if (category.Dishes.Any())
                throw new InvalidOperationException("Cannot delete category with dishes");

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Dishes

        public async Task<List<Dish>> GetAllDishesAsync()
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Dishes
                .Include(d => d.Category)
                .OrderBy(d => d.Category!.Name)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<List<Dish>> GetDishesByCategoryAsync(int categoryId)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Dishes
                .Include(d => d.Category)
                .Where(d => d.CategoryId == categoryId)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<List<Dish>> GetActiveDishesAsync()
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Dishes
                .Include(d => d.Category)
                .Where(d => d.IsActive)
                .OrderBy(d => d.Category!.Name)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Dish?> GetDishByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Dish> CreateDishAsync(Dish dish)
        {
            using var context = _dbContextFactory.CreateContext();
            dish.CreatedAt = DateTime.Now;
            context.Dishes.Add(dish);
            await context.SaveChangesAsync();
            return dish;
        }

        public async Task<Dish> UpdateDishAsync(Dish dish)
        {
            using var context = _dbContextFactory.CreateContext();
            var existing = await context.Dishes.FindAsync(dish.Id);

            if (existing == null)
                throw new InvalidOperationException("Dish not found");

            existing.Name = dish.Name;
            existing.Description = dish.Description;
            existing.Price = dish.Price;
            existing.CategoryId = dish.CategoryId;
            existing.IsActive = dish.IsActive;
            existing.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteDishAsync(int id)
        {
            using var context = _dbContextFactory.CreateContext();
            var dish = await context.Dishes
                .Include(d => d.OrderItems)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null)
                return false;

            if (dish.OrderItems.Any())
                throw new InvalidOperationException("Cannot delete dish that has been ordered");

            context.Dishes.Remove(dish);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleDishActiveAsync(int id)
        {
            using var context = _dbContextFactory.CreateContext();
            var dish = await context.Dishes.FindAsync(id);

            if (dish == null)
                return false;

            dish.IsActive = !dish.IsActive;
            dish.UpdatedAt = DateTime.Now;
            await context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
