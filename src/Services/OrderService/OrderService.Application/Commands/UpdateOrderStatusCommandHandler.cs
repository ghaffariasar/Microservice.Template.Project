using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Repositories;
using Shared.Common;
using Shared.Services;

namespace OrderService.Application.Commands;

/// <summary>
/// Handler برای تغییر وضعیت سفارش
/// از Distributed Lock برای اطمینان از صحت تغییرات استفاده می‌کند
/// </summary>
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IDistributedLockService lockService, ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _lockService = lockService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // استفاده از Distributed Lock
            var lockKey = $"order:update:{request.OrderId}";
            await using var lockHandle = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for order update: {OrderId}", request.OrderId);
                return Result.Failure<bool>("Unable to update order at this time. Please try again.");
            }

            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result.Failure<bool>($"Order with id {request.OrderId} not found");
            }

            order.ChangeStatus(request.NewStatus);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            _logger.LogInformation("Order status updated: {OrderId} to {Status}", request.OrderId, request.NewStatus);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status: {OrderId}", request.OrderId);
            return Result.Failure<bool>($"Error updating order status: {ex.Message}");
        }
    }
}

