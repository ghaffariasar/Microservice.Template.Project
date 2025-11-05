using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Domain.Repositories;
using Shared.Common;

namespace OrderService.Application.Queries;

/// <summary>
/// Handler برای دریافت تمام سفارش‌ها
/// </summary>
public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<List<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllOrdersQueryHandler> _logger;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper, ILogger<GetAllOrdersQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync(cancellationToken);

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            _logger.LogInformation("Retrieved {Count} orders", orderDtos.Count);
            return Result.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return Result.Failure<List<OrderDto>>($"Error retrieving orders: {ex.Message}");
        }
    }
}

