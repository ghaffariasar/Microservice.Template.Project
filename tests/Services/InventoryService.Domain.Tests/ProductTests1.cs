using InventoryService.Domain.Entities;
using Xunit;

namespace InventoryService.Domain.Tests
{
    public class ProductTests1
    {

        [Fact]
        public void Product_Check_Reserve_Value()
        {
            // Arrange
            var product = new Product("Test", "Product Desc", 1000, 10);

            // Act
            product.ReserveQuantity(7);

            // Assert
            Assert.Equal(7, product.ReservedQuantity);
        }



    }
}
