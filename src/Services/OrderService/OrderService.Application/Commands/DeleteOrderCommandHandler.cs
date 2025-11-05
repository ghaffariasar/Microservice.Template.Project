using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Repositories;
using Shared.Common;

namespace OrderService.Application.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger<DeleteOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Result.Failure<bool>("Order not found");

        var ok = await _orderRepository.DeleteAsync(request.OrderId, cancellationToken);
        if (!ok)
            return Result.Failure<bool>("Delete failed");

        _logger.LogInformation("Order deleted: {OrderId}", request.OrderId);
        return Result.Success(true);
    }
}


