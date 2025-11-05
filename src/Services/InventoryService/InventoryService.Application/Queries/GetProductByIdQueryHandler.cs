using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Repositories;
using Shared.Common;
using System.Text.Json;

namespace InventoryService.Application.Queries;

/// <summary>
/// Handler برای دریافت محصول بر اساس ID
/// از Cache برای بهبود عملکرد استفاده می‌کند
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedCache _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(IProductRepository productRepository, IDistributedCache cache, IMapper mapper, ILogger<GetProductByIdQueryHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // بررسی Cache
            var cacheKey = $"product:{request.ProductId}";
            var cachedProduct = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedProduct))
            {
                _logger.LogInformation("Product retrieved from cache: {ProductId}", request.ProductId);

                var product = JsonSerializer.Deserialize<ProductDto>(cachedProduct);
                return Result.Success(product!);
            }

            // دریافت از دیتابیس
            var productEntity = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (productEntity == null)
                return Result.Failure<ProductDto>($"Product with id {request.ProductId} not found");

            var productDto = _mapper.Map<ProductDto>(productEntity);

            // (ذخیره در کش (3 دقیقه - برای محصولات کمتر
            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3) };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(productDto), cacheOptions, cancellationToken);

            return Result.Success(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product: {ProductId}", request.ProductId);
            return Result.Failure<ProductDto>($"Error retrieving product: {ex.Message}");
        }
    }
}

