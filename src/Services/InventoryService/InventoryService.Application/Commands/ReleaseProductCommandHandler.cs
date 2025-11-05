using MediatR;
using Microsoft.Extensions.Logging;
using InventoryService.Domain.Repositories;
using Shared.Common;
using Shared.Services;

namespace InventoryService.Application.Commands;

/// <summary>
/// Handler برای آزادسازی موجودی رزرو شده
/// از Distributed Lock برای جلوگیری از Race Condition استفاده می‌کند
/// </summary>
public class ReleaseProductCommandHandler : IRequestHandler<ReleaseProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<ReleaseProductCommandHandler> _logger;

    public ReleaseProductCommandHandler(IProductRepository productRepository, IDistributedLockService lockService, ILogger<ReleaseProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _lockService = lockService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ReleaseProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // استفاده از Distributed Lock
            var lockKey = $"product:release:{request.ProductId}";
            await using var lockHandle = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for product release: {ProductId}", request.ProductId);
                return Result.Failure<bool>("Unable to release product at this time. Please try again.");
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure<bool>($"Product with id {request.ProductId} not found");
            }

            // آزادسازی موجودی
            product.ReleaseReservedQuantity(request.Quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Product released successfully: {ProductId}, Quantity: {Quantity}", request.ProductId, request.Quantity);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing product: {ProductId}", request.ProductId);
            return Result.Failure<bool>($"Error releasing product: {ex.Message}");
        }
    }
}

