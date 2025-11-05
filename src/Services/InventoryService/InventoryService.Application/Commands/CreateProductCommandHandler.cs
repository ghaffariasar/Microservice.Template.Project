using InventoryService.Domain.Entities;
using InventoryService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common;
using Shared.Services;

namespace InventoryService.Application.Commands;

/// <summary>
/// Handler برای ایجاد محصول
/// از Distributed Lock برای جلوگیری از Race Condition استفاده می‌کند
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IProductRepository productRepository, IDistributedLockService lockService, ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _lockService = lockService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Distributed Lock استفاده از 
            var lockKey = $"product:create:{request.Name.ToLowerInvariant()}";
            await using var lockHandle = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for product creation: {ProductName}", request.Name);
                return Result.Failure<Guid>("Unable to process product creation at this time. Please try again.");
            }

            // بررسی وجود محصول با همین نام
            var existingProduct = await _productRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingProduct != null)
                return Result.Failure<Guid>($"Product with name '{request.Name}' already exists");

            var product = new Product(request.Name, request.Description, request.Price, request.InitialStock);
            var createdProduct = await _productRepository.CreateAsync(product, cancellationToken);

            _logger.LogInformation("Product created successfully: {ProductId} - {ProductName}", createdProduct.Id, createdProduct.Name);

            return Result.Success(createdProduct.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
            return Result.Failure<Guid>($"Error creating product: {ex.Message}");
        }
    }
}

