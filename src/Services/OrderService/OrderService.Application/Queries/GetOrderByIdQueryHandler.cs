using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using OrderService.Application.DTOs;
using OrderService.Domain.Repositories;
using Shared.Common;
using System.Text.Json;

namespace OrderService.Application.Queries;

/// <summary>
/// Handler برای دریافت سفارش بر اساس ID
/// از Cache برای بهبود عملکرد استفاده می‌کند
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDistributedCache _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IDistributedCache cache, IMapper mapper, ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // بررسی Cache
            var cacheKey = $"order:{request.OrderId}";
            var cachedOrder = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedOrder))
            {
                _logger.LogInformation("Order retrieved from cache: {OrderId}", request.OrderId);
                var order = JsonSerializer.Deserialize<OrderDto>(cachedOrder);
                return Result.Success(order!);
            }

            // دریافت از دیتابیس
            var orderEntity = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (orderEntity == null)
                return Result.Failure<OrderDto>($"Order with id {request.OrderId} not found");

            var orderDto = _mapper.Map<OrderDto>(orderEntity);

            // ذخیره در Cache (5 دقیقه)
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(orderDto), cacheOptions, cancellationToken);

            return Result.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order: {OrderId}", request.OrderId);
            return Result.Failure<OrderDto>($"Error retrieving order: {ex.Message}");
        }
    }
}

