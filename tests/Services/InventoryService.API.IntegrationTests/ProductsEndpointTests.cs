using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InventoryService.API.IntegrationTests;

public class ProductsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public ProductsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_Should_Return_200()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/api/products");

        // Assert
        Assert.True(resp.IsSuccessStatusCode);
    }
}