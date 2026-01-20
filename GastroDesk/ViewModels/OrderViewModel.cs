using System.Collections.ObjectModel;
using System.Windows.Input;
using GastroDesk.Commands;
using GastroDesk.Models;
using GastroDesk.Models.Enums;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.ViewModels
{
    public class OrderViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;
        private readonly IMenuService _menuService;
        private readonly User _currentUser;

        public ObservableCollection<Order> Orders { get; } = new();
        public ObservableCollection<Dish> AvailableDishes { get; } = new();
        public ObservableCollection<OrderItem> CurrentOrderItems { get; } = new();

        private Order? _selectedOrder;
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                LoadOrderItems();
                OnPropertyChanged(nameof(CanModifyOrder));
            }
        }

        private Dish? _selectedDish;
        public Dish? SelectedDish
        {
            get => _selectedDish;
            set => SetProperty(ref _selectedDish, value);
        }

        private int _newTableNumber = 1;
        public int NewTableNumber
        {
            get => _newTableNumber;
            set => SetProperty(ref _newTableNumber, value);
        }

        private int _quantityToAdd = 1;
        public int QuantityToAdd
        {
            get => _quantityToAdd;
            set => SetProperty(ref _quantityToAdd, value);
        }

        private DateTime _filterDate = DateTime.Today;
        public DateTime FilterDate
        {
            get => _filterDate;
            set
            {
                SetProperty(ref _filterDate, value);
                _ = LoadOrdersAsync();
            }
        }

        private bool _showAllOrders;
        public bool ShowAllOrders
        {
            get => _showAllOrders;
            set
            {
                SetProperty(ref _showAllOrders, value);
                _ = LoadOrdersAsync();
            }
        }

        public bool CanModifyOrder => SelectedOrder?.Status == OrderStatus.Active;

        public decimal CurrentOrderTotal => CurrentOrderItems.Sum(i => i.TotalPrice);

        public ICommand LoadDataCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand AddItemToOrderCommand { get; }
        public ICommand RemoveItemFromOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand RefreshCommand { get; }

        public OrderViewModel(IOrderService orderService, IMenuService menuService, User currentUser)
        {
            _orderService = orderService;
            _menuService = menuService;
            _currentUser = currentUser;

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            CreateOrderCommand = new AsyncRelayCommand(CreateOrderAsync, () => NewTableNumber > 0);
            AddItemToOrderCommand = new AsyncRelayCommand(AddItemToOrderAsync, () => CanModifyOrder && SelectedDish != null && QuantityToAdd > 0);
            RemoveItemFromOrderCommand = new AsyncRelayCommand<OrderItem>(RemoveItemFromOrderAsync);
            CompleteOrderCommand = new AsyncRelayCommand(CompleteOrderAsync, () => CanModifyOrder && CurrentOrderItems.Any());
            CancelOrderCommand = new AsyncRelayCommand(CancelOrderAsync, () => CanModifyOrder);
            DeleteOrderCommand = new AsyncRelayCommand(DeleteOrderAsync, () => SelectedOrder != null);
            RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadDishesAsync();
            await LoadOrdersAsync();
        }

        private async Task LoadDishesAsync()
        {
            try
            {
                var dishes = await _menuService.GetActiveDishesAsync();
                AvailableDishes.Clear();
                foreach (var dish in dishes)
                {
                    AvailableDishes.Add(dish);
                }
            }
            catch (Exception ex)
            {
                SetError($"Error loading dishes: {ex.Message}");
            }
        }

        private async Task LoadOrdersAsync()
        {
            IsLoading = true;
            try
            {
                List<Order> orders;
                if (ShowAllOrders)
                {
                    orders = await _orderService.GetAllOrdersAsync();
                }
                else
                {
                    orders = await _orderService.GetOrdersByDateAsync(FilterDate);
                }

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                SetError($"Error loading orders: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadOrderItems()
        {
            CurrentOrderItems.Clear();
            if (SelectedOrder?.Items != null)
            {
                foreach (var item in SelectedOrder.Items)
                {
                    CurrentOrderItems.Add(item);
                }
            }
            OnPropertyChanged(nameof(CurrentOrderTotal));
        }

        private async Task CreateOrderAsync()
        {
            try
            {
                var order = new Order
                {
                    TableNumber = NewTableNumber,
                    UserId = _currentUser.Id,
                    Status = OrderStatus.Active
                };

                var createdOrder = await _orderService.CreateOrderAsync(order);
                await LoadOrdersAsync();

                SelectedOrder = Orders.FirstOrDefault(o => o.Id == createdOrder.Id);
                NewTableNumber = 1;
            }
            catch (Exception ex)
            {
                SetError($"Error creating order: {ex.Message}");
            }
        }

        private async Task AddItemToOrderAsync()
        {
            if (SelectedOrder == null || SelectedDish == null) return;

            try
            {
                await _orderService.AddItemToOrderAsync(SelectedOrder.Id, SelectedDish.Id, QuantityToAdd);

                var updatedOrder = await _orderService.GetOrderByIdAsync(SelectedOrder.Id);
                if (updatedOrder != null)
                {
                    var index = Orders.IndexOf(SelectedOrder);
                    Orders[index] = updatedOrder;
                    SelectedOrder = updatedOrder;
                }

                QuantityToAdd = 1;
            }
            catch (Exception ex)
            {
                SetError($"Error adding item: {ex.Message}");
            }
        }

        private async Task RemoveItemFromOrderAsync(OrderItem? item)
        {
            if (item == null || SelectedOrder == null) return;

            try
            {
                await _orderService.RemoveItemFromOrderAsync(item.Id);

                var updatedOrder = await _orderService.GetOrderByIdAsync(SelectedOrder.Id);
                if (updatedOrder != null)
                {
                    var index = Orders.IndexOf(SelectedOrder);
                    Orders[index] = updatedOrder;
                    SelectedOrder = updatedOrder;
                }
            }
            catch (Exception ex)
            {
                SetError($"Error removing item: {ex.Message}");
            }
        }

        private async Task CompleteOrderAsync()
        {
            if (SelectedOrder == null) return;

            try
            {
                await _orderService.ChangeOrderStatusAsync(SelectedOrder.Id, OrderStatus.Completed);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error completing order: {ex.Message}");
            }
        }

        private async Task CancelOrderAsync()
        {
            if (SelectedOrder == null) return;

            try
            {
                await _orderService.ChangeOrderStatusAsync(SelectedOrder.Id, OrderStatus.Cancelled);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error cancelling order: {ex.Message}");
            }
        }

        private async Task DeleteOrderAsync()
        {
            if (SelectedOrder == null) return;

            try
            {
                await _orderService.DeleteOrderAsync(SelectedOrder.Id);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                SetError($"Error deleting order: {ex.Message}");
            }
        }
    }
}
