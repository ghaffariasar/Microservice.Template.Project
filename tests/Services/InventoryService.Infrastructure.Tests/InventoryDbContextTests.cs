using Microsoft.EntityFrameworkCore;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Xunit;

namespace InventoryService.Infrastructure.Tests;

public class InventoryDbContextTests
{
    [Fact]
    public async Task Save_Product_InMemory()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new InventoryDbContext(options);
        var product = new Product("N", "D", 10m, 5);
        ctx.Products.Add(product);

        // Act
        await ctx.SaveChangesAsync();
        var loaded = await ctx.Products.FirstOrDefaultAsync(p => p.Id == product.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("N", loaded!.Name);
    }
}