using MediatR;
using Shared.Common;

namespace InventoryService.Application.Commands;

/// <summary>
/// کامند آزادسازی موجودی رزرو شده محصول
/// </summary>
public class ReleaseProductCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

