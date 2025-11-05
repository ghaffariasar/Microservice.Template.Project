using MediatR;
using OrderService.Application.DTOs;
using Shared.Common;

namespace OrderService.Application.Queries;

/// <summary>
/// Query برای دریافت سفارش بر اساس ID
/// </summary>
public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; set; }
}

