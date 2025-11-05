using MediatR;
using Microsoft.Extensions.Logging;
using InventoryService.Domain.Repositories;
using Shared.Common;
using Shared.Services;

namespace InventoryService.Application.Commands;

/// <summary>
/// Handler برای رزرو موجودی محصول با Idempotency و Distributed Lock.
/// </summary>
public class ReserveProductCommandHandler : IRequestHandler<ReserveProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedLockService _lockService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<ReserveProductCommandHandler> _logger;

    public ReserveProductCommandHandler(IProductRepository productRepository, IDistributedLockService lockService, IIdempotencyService idempotencyService, ILogger<ReserveProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _lockService = lockService;
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ReserveProductCommand request, CancellationToken cancellationToken)
    {
        // بررسی Idempotency
        var idempotentResult = await TryHandleIdempotencyAsync(request, cancellationToken);
        if (idempotentResult is not null)
            return idempotentResult;

        // Retry
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var result = await TryReserveProductAsync(request, attempt, cancellationToken);
            if (result.IsSuccess)
            {
                await SaveIdempotentResultAsync(request, result.Value, cancellationToken);
                return result;
            }

            // در صورت خطای موقتی، تلاش مجدد
            if (result.ErrorMessage.Contains("concurrent", StringComparison.OrdinalIgnoreCase))
                await Task.Delay(100 * attempt, cancellationToken);
            else
                return result;
        }

        return Result.Failure<bool>("Unable to reserve product after multiple attempts.");
    }

    private async Task<Result<bool>?> TryHandleIdempotencyAsync(ReserveProductCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.IdempotencyKey))
            return null;

        var cached = await _idempotencyService.GetValueAsync(request.IdempotencyKey, cancellationToken);
        if (string.IsNullOrEmpty(cached))
            return null;

        var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(cached);
        if (parsed is null || !parsed.TryGetValue("Success", out var s) || !bool.TryParse(s.ToString(), out var success) || !success) 
            return null;

        _logger.LogInformation("Returning cached result for key {Key}", request.IdempotencyKey);
        return Result.Success(true);
    }

    private async Task<Result<bool>> TryReserveProductAsync(ReserveProductCommand request, int attempt, CancellationToken cancellationToken)
    {
        var lockKey = $"product:reserve:{request.ProductId}";
        await using var lockHandle = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Lock acquisition failed for Product {ProductId}, attempt {Attempt}", request.ProductId, attempt);
            return Result.Failure<bool>("Could not acquire distributed lock.");
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            return Result.Failure<bool>("Product not found.");

        if (product.AvailableQuantity < request.Quantity)
            return Result.Failure<bool>($"Insufficient quantity. Available: {product.AvailableQuantity}");

        product.ReserveQuantity(request.Quantity);

        try
        {
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Reserved {Qty} units of Product {ProductId}", request.Quantity, request.ProductId);
            return Result.Success(true);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict for Product {ProductId}, attempt {Attempt}", request.ProductId, attempt);
            return Result.Failure<bool>("concurrent update conflict");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reserving Product {ProductId}", request.ProductId);
            return Result.Failure<bool>(ex.Message);
        }
    }

    private async Task SaveIdempotentResultAsync(ReserveProductCommand request, bool success, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.IdempotencyKey)) 
            return;

        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            Success = success,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            ReservedAt = DateTime.UtcNow
        });

        await _idempotencyService.SetValueAsync(request.IdempotencyKey, json, TimeSpan.FromHours(24), cancellationToken);
    }
}
