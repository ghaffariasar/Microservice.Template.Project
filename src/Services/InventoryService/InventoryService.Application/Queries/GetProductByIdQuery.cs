using MediatR;
using InventoryService.Application.DTOs;
using Shared.Common;

namespace InventoryService.Application.Queries;

/// <summary>
/// Query برای دریافت محصول بر اساس ID
/// </summary>
public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid ProductId { get; set; }
}

