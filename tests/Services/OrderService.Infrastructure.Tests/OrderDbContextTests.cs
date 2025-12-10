using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data;
using Xunit;

namespace OrderService.Infrastructure.Tests;

public class OrderDbContextTests
{
    [Fact]
    public async Task Save_Order_With_Items_InMemory()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new OrderDbContext(options);
        var order = new Order(Guid.NewGuid(), new List<OrderItem>());
        order.AddItem(new OrderItem(Guid.Empty, Guid.NewGuid(), "P", 1, 9m));
        ctx.Orders.Add(order);

        // Act
        await ctx.SaveChangesAsync();
        var loaded = await ctx.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == order.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Items);
    }
}