using MediatR;
using Shared.Common;

namespace InventoryService.Application.Commands;

/// <summary>
/// Command برای تایید نهایی سفارش و کاهـش موجودی رزروشده از موجودی کل.
/// </summary>
public class ConfirmProductCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? IdempotencyKey { get; set; }
}


