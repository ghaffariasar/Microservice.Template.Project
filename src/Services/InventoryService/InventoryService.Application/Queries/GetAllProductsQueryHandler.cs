using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Repositories;
using Shared.Common;

namespace InventoryService.Application.Queries;

/// <summary>
/// Handler برای دریافت تمام محصولات
/// </summary>
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper, ILogger<GetAllProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productRepository.GetAllAsync(cancellationToken);

            var productDtos = _mapper.Map<List<ProductDto>>(products);

            _logger.LogInformation("Retrieved {Count} products", productDtos.Count);
            return Result.Success(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return Result.Failure<List<ProductDto>>($"Error retrieving products: {ex.Message}");
        }
    }
}

