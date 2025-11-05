using MediatR;
using Microsoft.Extensions.Logging;
using InventoryService.Domain.Repositories;
using Shared.Common;
using Shared.Services;

namespace InventoryService.Application.Commands;

/// <summary>
/// Handler برای تایید نهایی رزرو و کاهش موجودی واقعی
/// از Distributed Lock برای جلوگیری از Race Condition استفاده می‌کند
/// و قابلیت Idempotency را نیز پشتیبانی می‌کند.
/// </summary>
public class ConfirmProductCommandHandler : IRequestHandler<ConfirmProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedLockService _lockService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<ConfirmProductCommandHandler> _logger;

    public ConfirmProductCommandHandler(IProductRepository productRepository, IDistributedLockService lockService, IIdempotencyService idempotencyService, ILogger<ConfirmProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _lockService = lockService;
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ConfirmProductCommand request, CancellationToken cancellationToken)
    {
        // Idempotency check
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _idempotencyService.GetValueAsync(request.IdempotencyKey, cancellationToken);
            if (!string.IsNullOrEmpty(existing))
            {
                _logger.LogInformation("ConfirmProduct idempotent hit for {ProductId}", request.ProductId);
                return Result.Success(true);
            }
        }

        try
        {
            var lockKey = $"product:commit:{request.ProductId}";
            await using var lockHandle = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for product commit: {ProductId}", request.ProductId);
                return Result.Failure<bool>("Unable to confirm product at this time. Please try again.");
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure<bool>($"Product with id {request.ProductId} not found");
            }

            // کاهش موجودی واقعی از رزروشده
            product.DecreaseStock(request.Quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
                await _idempotencyService.SetValueAsync(request.IdempotencyKey, "confirmed", TimeSpan.FromHours(24), cancellationToken);

            _logger.LogInformation("Product committed successfully: {ProductId}, Quantity: {Quantity}", request.ProductId, request.Quantity);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming product: {ProductId}", request.ProductId);
            return Result.Failure<bool>($"Error confirming product: {ex.Message}");
        }
    }
}


