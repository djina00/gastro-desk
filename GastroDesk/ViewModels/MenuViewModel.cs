using System.Collections.ObjectModel;
using System.Windows.Input;
using GastroDesk.Commands;
using GastroDesk.Models;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IMenuService _menuService;
        // private readonly IReportService _reportService; // Will be added later for export/import
        private readonly bool _isManager;

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Dish> Dishes { get; } = new();

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                _ = LoadDishesAsync();
            }
        }

        private Dish? _selectedDish;
        public Dish? SelectedDish
        {
            get => _selectedDish;
            set => SetProperty(ref _selectedDish, value);
        }

        private string _newCategoryName = string.Empty;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set => SetProperty(ref _newCategoryName, value);
        }

        private string _newCategoryDescription = string.Empty;
        public string NewCategoryDescription
        {
            get => _newCategoryDescription;
            set => SetProperty(ref _newCategoryDescription, value);
        }

        private string _dishName = string.Empty;
        public string DishName
        {
            get => _dishName;
            set => SetProperty(ref _dishName, value);
        }

        private string _dishDescription = string.Empty;
        public string DishDescription
        {
            get => _dishDescription;
            set => SetProperty(ref _dishDescription, value);
        }

        private decimal _dishPrice;
        public decimal DishPrice
        {
            get => _dishPrice;
            set => SetProperty(ref _dishPrice, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsManager => _isManager;

        public ICommand LoadDataCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand AddDishCommand { get; }
        public ICommand EditDishCommand { get; }
        public ICommand SaveDishCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteDishCommand { get; }
        public ICommand ToggleDishActiveCommand { get; }
        // Export/Import commands will be added later with ReportService
        // public ICommand ExportToJsonCommand { get; }
        // public ICommand ExportToXmlCommand { get; }
        // public ICommand ImportFromJsonCommand { get; }
        // public ICommand ImportFromXmlCommand { get; }

        public MenuViewModel(IMenuService menuService, bool isManager)
        {
            _menuService = menuService;
            _isManager = isManager;

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync, () => _isManager && !string.IsNullOrWhiteSpace(NewCategoryName));
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync, () => _isManager && SelectedCategory != null);
            AddDishCommand = new RelayCommand(StartAddDish, () => _isManager && SelectedCategory != null);
            EditDishCommand = new RelayCommand(StartEditDish, () => _isManager && SelectedDish != null);
            SaveDishCommand = new AsyncRelayCommand(SaveDishAsync, () => _isManager && !string.IsNullOrWhiteSpace(DishName) && DishPrice > 0);
            CancelEditCommand = new RelayCommand(CancelEdit);
            DeleteDishCommand = new AsyncRelayCommand(DeleteDishAsync, () => _isManager && SelectedDish != null);
            ToggleDishActiveCommand = new AsyncRelayCommand(ToggleDishActiveAsync, () => _isManager && SelectedDish != null);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var categories = await _menuService.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                if (SelectedCategory == null && Categories.Any())
                {
                    SelectedCategory = Categories.First();
                }
            }
            catch (Exception ex)
            {
                SetError($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDishesAsync()
        {
            Dishes.Clear();
            if (SelectedCategory == null) return;

            try
            {
                var dishes = await _menuService.GetDishesByCategoryAsync(SelectedCategory.Id);
                foreach (var dish in dishes)
                {
                    Dishes.Add(dish);
                }
            }
            catch (Exception ex)
            {
                SetError($"Error loading dishes: {ex.Message}");
            }
        }

        private async Task AddCategoryAsync()
        {
            try
            {
                var category = new Category
                {
                    Name = NewCategoryName,
                    Description = NewCategoryDescription
                };

                await _menuService.CreateCategoryAsync(category);
                NewCategoryName = string.Empty;
                NewCategoryDescription = string.Empty;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error adding category: {ex.Message}");
            }
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;

            try
            {
                await _menuService.DeleteCategoryAsync(SelectedCategory.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error deleting category: {ex.Message}");
            }
        }

        private void StartAddDish()
        {
            SelectedDish = null;
            DishName = string.Empty;
            DishDescription = string.Empty;
            DishPrice = 0;
            IsEditing = true;
        }

        private void StartEditDish()
        {
            if (SelectedDish == null) return;
            DishName = SelectedDish.Name;
            DishDescription = SelectedDish.Description ?? string.Empty;
            DishPrice = SelectedDish.Price;
            IsEditing = true;
        }

        private async Task SaveDishAsync()
        {
            if (SelectedCategory == null) return;

            try
            {
                if (SelectedDish == null)
                {
                    var dish = new Dish
                    {
                        Name = DishName,
                        Description = DishDescription,
                        Price = DishPrice,
                        CategoryId = SelectedCategory.Id,
                        IsActive = true
                    };
                    await _menuService.CreateDishAsync(dish);
                }
                else
                {
                    SelectedDish.Name = DishName;
                    SelectedDish.Description = DishDescription;
                    SelectedDish.Price = DishPrice;
                    await _menuService.UpdateDishAsync(SelectedDish);
                }

                IsEditing = false;
                await LoadDishesAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error saving dish: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            DishName = string.Empty;
            DishDescription = string.Empty;
            DishPrice = 0;
        }

        private async Task DeleteDishAsync()
        {
            if (SelectedDish == null) return;

            try
            {
                await _menuService.DeleteDishAsync(SelectedDish.Id);
                await LoadDishesAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error deleting dish: {ex.Message}");
            }
        }

        private async Task ToggleDishActiveAsync()
        {
            if (SelectedDish == null) return;

            try
            {
                await _menuService.ToggleDishActiveAsync(SelectedDish.Id);
                await LoadDishesAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error toggling dish status: {ex.Message}");
            }
        }
    }
}
