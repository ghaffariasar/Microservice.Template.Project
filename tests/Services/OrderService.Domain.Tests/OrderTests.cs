using System;
using System.Collections.Generic;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void AddItem_Should_Update_Total_And_Set_OrderId()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), new List<OrderItem>());
        var item = new OrderItem(Guid.Empty, Guid.NewGuid(), "P", 2, 10m);

        // Act
        order.AddItem(item);

        // Assert
        Assert.Equal(order.Id, item.OrderId);
        Assert.Equal(20m, order.TotalAmount);
        Assert.Single(order.Items);
    }

    [Fact]
    public void ChangeStatus_Should_Throw_When_Completed()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), new List<OrderItem>());

        // Act
        order.ChangeStatus(OrderStatus.Completed);

        // Assert
        Assert.Throws<InvalidOperationException>(() => order.ChangeStatus(OrderStatus.Confirmed));
    }
}