using MediatR;
using OrderService.Domain.Entities;
using Shared.Common;

namespace OrderService.Application.Commands;

/// <summary>
/// کامند تغییر وضعیت سفارش
/// </summary>
public class UpdateOrderStatusCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}

