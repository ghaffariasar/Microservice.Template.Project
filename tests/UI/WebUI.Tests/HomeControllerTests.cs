using Microsoft.AspNetCore.Mvc;
using WebUI.Controllers;
using Xunit;

namespace WebUI.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_Returns_View()
    {
        // Arrange
        var c = new HomeController();

        // Act
        var r = c.Index();

        // Assert
        Assert.IsType<ViewResult>(r);
    }
}