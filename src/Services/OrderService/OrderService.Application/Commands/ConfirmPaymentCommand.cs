using MediatR;
using Shared.Common;

namespace OrderService.Application.Commands;

/// <summary>
/// Command برای تایید پرداخت و نهایی‌کردن موجودی رزرو شده سفارش
/// </summary>
public class ConfirmPaymentCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public string? IdempotencyKey { get; set; }
}


