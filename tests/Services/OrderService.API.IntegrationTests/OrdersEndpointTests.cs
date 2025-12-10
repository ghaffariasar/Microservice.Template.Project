using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace OrderService.API.IntegrationTests;

public class OrdersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public OrdersEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllOrders_Should_Return_200()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/api/orders");

        // Assert
        Assert.True(resp.IsSuccessStatusCode);
    }
}