using InventoryService.Domain.Entities;
using Xunit;

namespace InventoryService.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void ReserveQuantity_Should_Increase_Reserved_And_Decrease_Available()
    {
        // Arrange
        var p = new Product("N", "D", 100m, 10);

        // Act
        p.ReserveQuantity(3);

        // Assert
        Assert.Equal(3, p.ReservedQuantity);
        Assert.Equal(7, p.AvailableQuantity);
    }

    [Fact]
    public void ReleaseReservedQuantity_Should_Decrease_Reserved()
    {
        // Arrange
        var p = new Product("N", "D", 100m, 10);
        p.ReserveQuantity(5);

        // Act
        p.ReleaseReservedQuantity(2);

        // Assert
        Assert.Equal(3, p.ReservedQuantity);
    }
}