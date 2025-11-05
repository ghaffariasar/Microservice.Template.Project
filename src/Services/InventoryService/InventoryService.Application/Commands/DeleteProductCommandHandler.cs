using MediatR;
using Microsoft.Extensions.Logging;
using InventoryService.Domain.Repositories;
using Shared.Common;

namespace InventoryService.Application.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IProductRepository productRepository, ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            return Result.Failure<bool>("Product not found");

        var ok = await _productRepository.DeleteAsync(request.ProductId, cancellationToken);
        if (!ok)
            return Result.Failure<bool>("Delete failed");

        _logger.LogInformation("Product deleted: {ProductId}", request.ProductId);
        return Result.Success(true);
    }
}


