using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrderService.Application.Commands;
using OrderService.Domain.Repositories;
using Shared.Configuration;
using Shared.Services;
using Xunit;

namespace OrderService.Application.Tests;

public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Existing_Order_When_Idempotent()
    {
        // Arrange
        var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
        var lockService = new Mock<IDistributedLockService>(MockBehavior.Strict);
        var idemp = new Mock<IIdempotencyService>(MockBehavior.Strict);
        var httpFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = new Mock<ILogger<CreateOrderCommandHandler>>();
        var invOpts = Options.Create(new InventoryServiceOptions
        {
            BaseUrl = "http://localhost",
            Endpoints = new InventoryEndpoints { ReserveProduct = "/reserve/{id}", ReleaseProduct = "/release/{id}", CommitProduct = "/commit/{id}" }
        });
        var gwOpts = Options.Create(new GatewayOptions { ApiKey = "k" });

        var cached = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object> { { "OrderId", Guid.NewGuid().ToString() } });
        idemp.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(cached);
        idemp.Setup(x => x.IsProcessingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        idemp.Setup(x => x.MarkAsProcessingAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new CreateOrderCommandHandler(orderRepo.Object, lockService.Object, idemp.Object, httpFactory.Object, config, logger.Object, invOpts, gwOpts);

        var cmd = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto> { new() { ProductId = Guid.NewGuid(), ProductName = "P", Quantity = 1, UnitPrice = 1m } },
            IdempotencyKey = "key-1"
        };

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }
}