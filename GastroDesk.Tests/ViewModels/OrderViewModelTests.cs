using GastroDesk.Models;
using GastroDesk.Models.Enums;

namespace GastroDesk.Tests.ViewModels;

public class OrderViewModelTests
{
    [Fact]
    public void Order_CalculateTotalPrice_ReturnsCorrectSum()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            TableNumber = 5,
            Status = OrderStatus.Active
        };

        order.Items.Add(new OrderItem { Price = 10.00m, Quantity = 2 }); // 20.00
        order.Items.Add(new OrderItem { Price = 5.50m, Quantity = 3 });  // 16.50
        order.Items.Add(new OrderItem { Price = 8.25m, Quantity = 1 });  // 8.25

        // Act
        var total = order.TotalPrice;

        // Assert
        Assert.Equal(44.75m, total);
    }

    [Fact]
    public void OrderItem_TotalPrice_ReturnsCorrectValue()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Price = 15.99m,
            Quantity = 3
        };

        // Act
        var total = orderItem.TotalPrice;

        // Assert
        Assert.Equal(47.97m, total);
    }

    [Fact]
    public void Order_EmptyItems_TotalPriceIsZero()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            TableNumber = 1,
            Status = OrderStatus.Active
        };

        // Act
        var total = order.TotalPrice;

        // Assert
        Assert.Equal(0m, total);
    }
}
