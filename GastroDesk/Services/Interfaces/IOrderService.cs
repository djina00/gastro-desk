using GastroDesk.Models;
using GastroDesk.Models.Enums;

namespace GastroDesk.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetOrdersByDateAsync(DateTime date);
        Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<List<Order>> GetOrdersByUserAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
        Task<Order> ChangeOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task<OrderItem> AddItemToOrderAsync(int orderId, int dishId, int quantity);
        Task<bool> RemoveItemFromOrderAsync(int orderItemId);
        Task<OrderItem> UpdateOrderItemQuantityAsync(int orderItemId, int quantity);
    }
}
