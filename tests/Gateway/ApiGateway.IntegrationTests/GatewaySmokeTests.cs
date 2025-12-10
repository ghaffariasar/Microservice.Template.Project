using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiGateway.IntegrationTests;

public class GatewaySmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public GatewaySmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Swagger_Index_Should_Return_200()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/swagger/index.html");

        // Assert
        Assert.True(resp.IsSuccessStatusCode);
    }
}