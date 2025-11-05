using MediatR;
using OrderService.Application.DTOs;
using Shared.Common;

namespace OrderService.Application.Queries;

/// <summary>
/// Query برای دریافت تمام سفارش‌ها
/// </summary>
public class GetAllOrdersQuery : IRequest<Result<List<OrderDto>>>
{
}

