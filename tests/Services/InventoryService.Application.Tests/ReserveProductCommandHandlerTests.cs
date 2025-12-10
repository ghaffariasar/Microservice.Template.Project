using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryService.Application.Commands;
using InventoryService.Domain.Repositories;
using Shared.Services;
using Xunit;

namespace InventoryService.Application.Tests;

public class ReserveProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Cached_Success_When_Idempotent()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var lockSvc = new Mock<IDistributedLockService>(MockBehavior.Strict);
        var idemp = new Mock<IIdempotencyService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<ReserveProductCommandHandler>>();

        var json = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>{{"Success", true}});
        idemp.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(json);

        var handler = new ReserveProductCommandHandler(repo.Object, lockSvc.Object, idemp.Object, logger.Object);
        var cmd = new ReserveProductCommand{ ProductId = Guid.NewGuid(), Quantity = 1, IdempotencyKey = "k" };

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }
}