using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiGateway.IntegrationTests
{
    public class GatewayRunTests(WebApplicationFactory<Program> webApplicationFactory)  :IClassFixture<WebApplicationFactory<Program>>
    {

        [Fact]
        public async Task Swagger_Index_Should_Return_200()
        {
            // Arrange
            var client = webApplicationFactory.CreateClient();

            //Act
            var result = await client.GetAsync("/swagger/index.html");

            // Assert
            Assert.True(result.IsSuccessStatusCode);
        }

    }
}
