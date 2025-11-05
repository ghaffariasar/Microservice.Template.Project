using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Repositories;
using Shared.Common;
using Shared.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Shared.Services;

namespace OrderService.Application.Commands;

/// <summary>
/// Handler تایید پرداخت: برای هر آیتم سفارش، commit موجودی را در InventoryService فراخوانی می‌کند
/// سپس وضعیت سفارش را به Confirmed تغییر می‌دهد.
/// </summary>
public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly InventoryServiceOptions _inventoryOptions;
    private readonly GatewayOptions _gatewayOptions;
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<ConfirmPaymentCommandHandler> _logger;

    public ConfirmPaymentCommandHandler(IOrderRepository orderRepository, IHttpClientFactory httpClientFactory, IOptions<InventoryServiceOptions> inventoryOptions, IOptions<GatewayOptions> gatewayOptions, IIdempotencyService idempotencyService, ILogger<ConfirmPaymentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _httpClientFactory = httpClientFactory;
        _inventoryOptions = inventoryOptions.Value;
        _gatewayOptions = gatewayOptions.Value;
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        // Idempotency
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _idempotencyService.GetValueAsync(request.IdempotencyKey, cancellationToken);
            if (!string.IsNullOrEmpty(existing))
            {
                _logger.LogInformation("ConfirmPayment idempotent hit for order {OrderId}", request.OrderId);
                return Result.Success(true);
            }
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Result.Failure<bool>("Order not found");

        if (order.Status is Domain.Entities.OrderStatus.Cancelled or Domain.Entities.OrderStatus.Completed)
            return Result.Failure<bool>($"Order status is {order.Status} and cannot be confirmed");

        if (string.IsNullOrWhiteSpace(_inventoryOptions.BaseUrl) || string.IsNullOrWhiteSpace(_inventoryOptions.Endpoints?.CommitProduct))
            return Result.Failure<bool>("InventoryService commit endpoint is not configured");

        var httpClient = _httpClientFactory.CreateClient("DefaultClient");
        if (httpClient.BaseAddress == null)
            httpClient.BaseAddress = new Uri(_inventoryOptions.BaseUrl);

        var gatewayApiKey = _gatewayOptions.ApiKey ?? string.Empty;
        if (!string.IsNullOrEmpty(gatewayApiKey) && !httpClient.DefaultRequestHeaders.Contains(HeaderNames.GatewayApiKey))
            httpClient.DefaultRequestHeaders.Add(HeaderNames.GatewayApiKey, gatewayApiKey);

        foreach (var item in order.Items)
        {
            var commitIdempotencyKey = $"{request.IdempotencyKey ?? Guid.NewGuid().ToString()}-order-{order.Id}-product-{item.ProductId}";
            var commitPath = _inventoryOptions.Endpoints!.CommitProduct!.Replace("{id}", item.ProductId.ToString());
            
            var commitRequest = new
            {
                Quantity = item.Quantity, 
                IdempotencyKey = commitIdempotencyKey
            };

            var response = await httpClient.PostAsJsonAsync(commitPath, commitRequest, cancellationToken);
            if (response.IsSuccessStatusCode) 
                continue;

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Commit failed for product {ProductId} in order {OrderId}: {Error}", item.ProductId, order.Id, errorContent);
            return Result.Failure<bool>("Commit failed for some items");
        }

        order.ChangeStatus(Domain.Entities.OrderStatus.Confirmed);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        if (!string.IsNullOrEmpty(request.IdempotencyKey))
            await _idempotencyService.SetValueAsync(request.IdempotencyKey, "confirmed", TimeSpan.FromHours(24), cancellationToken);

        return Result.Success(true);
    }
}


