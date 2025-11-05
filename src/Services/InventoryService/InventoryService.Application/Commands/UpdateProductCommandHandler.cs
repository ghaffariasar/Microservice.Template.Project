using MediatR;
using Microsoft.Extensions.Logging;
using InventoryService.Domain.Repositories;
using Shared.Common;

namespace InventoryService.Application.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(IProductRepository productRepository, ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            return Result.Failure<bool>("Product not found");

        product.Update(request.Name, request.Description, request.Price);

        await _productRepository.UpdateAsync(product, cancellationToken);
        _logger.LogInformation("Product updated: {ProductId}", product.Id);
        return Result.Success(true);
    }
}


