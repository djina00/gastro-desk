using Microsoft.EntityFrameworkCore;
using GastroDesk.Data;
using GastroDesk.Models;
using GastroDesk.Models.Enums;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.Services
{
    public class OrderService : IOrderService
    {
        private readonly DbContextFactory _dbContextFactory;

        public OrderService()
        {
            _dbContextFactory = DbContextFactory.Instance;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                .OrderByDescending(o => o.OrderDateTime)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByDateAsync(DateTime date)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                .Where(o => o.OrderDateTime.Date == date.Date)
                .OrderByDescending(o => o.OrderDateTime)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                .Where(o => o.OrderDateTime.Date >= startDate.Date && o.OrderDateTime.Date <= endDate.Date)
                .OrderByDescending(o => o.OrderDateTime)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDateTime)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByUserAsync(int userId)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDateTime)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateContext();
            return await context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            using var context = _dbContextFactory.CreateContext();
            order.CreatedAt = DateTime.Now;
            order.OrderDateTime = DateTime.Now;
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            using var context = _dbContextFactory.CreateContext();
            var existing = await context.Orders.FindAsync(order.Id);

            if (existing == null)
                throw new InvalidOperationException("Order not found");

            existing.TableNumber = order.TableNumber;
            existing.Notes = order.Notes;
            existing.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            using var context = _dbContextFactory.CreateContext();
            var order = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return false;

            context.OrderItems.RemoveRange(order.Items);
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Order> ChangeOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            using var context = _dbContextFactory.CreateContext();
            var order = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            return order;
        }

        public async Task<OrderItem> AddItemToOrderAsync(int orderId, int dishId, int quantity)
        {
            using var context = _dbContextFactory.CreateContext();
            var order = await context.Orders.FindAsync(orderId);
            var dish = await context.Dishes.FindAsync(dishId);

            if (order == null)
                throw new InvalidOperationException("Order not found");
            if (dish == null)
                throw new InvalidOperationException("Dish not found");

            var existingItem = await context.OrderItems
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.DishId == dishId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.Now;
                await context.SaveChangesAsync();
                return existingItem;
            }

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                DishId = dishId,
                Quantity = quantity,
                Price = dish.Price,
                CreatedAt = DateTime.Now
            };

            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();
            return orderItem;
        }

        public async Task<bool> RemoveItemFromOrderAsync(int orderItemId)
        {
            using var context = _dbContextFactory.CreateContext();
            var orderItem = await context.OrderItems.FindAsync(orderItemId);

            if (orderItem == null)
                return false;

            context.OrderItems.Remove(orderItem);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<OrderItem> UpdateOrderItemQuantityAsync(int orderItemId, int quantity)
        {
            using var context = _dbContextFactory.CreateContext();
            var orderItem = await context.OrderItems.FindAsync(orderItemId);

            if (orderItem == null)
                throw new InvalidOperationException("Order item not found");

            orderItem.Quantity = quantity;
            orderItem.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            return orderItem;
        }
    }
}
