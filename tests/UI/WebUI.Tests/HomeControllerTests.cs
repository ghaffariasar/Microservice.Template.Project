using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebUI.Controllers;
using Xunit;

namespace WebUI.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_Returns_View()
    {
        // Arrange
        var logger = NullLogger<HomeController>.Instance;
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var controller = new HomeController(
            logger,
            httpClientFactoryMock.Object
        );

        // Act
        var r = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(r);
    }
}