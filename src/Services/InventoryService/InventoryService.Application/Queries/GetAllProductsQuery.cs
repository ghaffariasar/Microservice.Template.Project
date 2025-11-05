using MediatR;
using InventoryService.Application.DTOs;
using Shared.Common;

namespace InventoryService.Application.Queries;

/// <summary>
/// Query برای دریافت تمام محصولات
/// </summary>
public class GetAllProductsQuery : IRequest<Result<List<ProductDto>>>
{
}

