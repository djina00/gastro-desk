using GastroDesk.Models;

namespace GastroDesk.Services.Interfaces
{
    public interface IMenuService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);

        Task<List<Dish>> GetAllDishesAsync();
        Task<List<Dish>> GetDishesByCategoryAsync(int categoryId);
        Task<List<Dish>> GetActiveDishesAsync();
        Task<Dish?> GetDishByIdAsync(int id);
        Task<Dish> CreateDishAsync(Dish dish);
        Task<Dish> UpdateDishAsync(Dish dish);
        Task<bool> DeleteDishAsync(int id);
        Task<bool> ToggleDishActiveAsync(int id);
    }
}
