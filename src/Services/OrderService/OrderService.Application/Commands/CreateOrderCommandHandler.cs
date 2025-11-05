using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using Shared.Common;
using Shared.Services;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shared.Configuration;

namespace OrderService.Application.Commands;

/// <summary>
/// Handler برای کامند ایجاد سفارش
/// این Handler از Distributed Lock برای جلوگیری از Race Condition استفاده می‌کند
/// همچنین موجودی را از Inventory Service رزرو می‌کند (Saga Pattern)
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDistributedLockService _lockService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly InventoryServiceOptions _inventoryOptions;
    private readonly GatewayOptions _gatewayOptions;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IDistributedLockService lockService, IIdempotencyService idempotencyService, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CreateOrderCommandHandler> logger, IOptions<InventoryServiceOptions> inventoryOptions, IOptions<GatewayOptions> gatewayOptions)
    {
        _orderRepository = orderRepository;
        _lockService = lockService;
        _idempotencyService = idempotencyService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _inventoryOptions = inventoryOptions.Value;
        _gatewayOptions = gatewayOptions.Value;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var reservedProducts = new List<(Guid ProductId, int Quantity)>();
        var orderId = Guid.Empty;

        // بررسی Idempotency Key - اگر Key وجود داشت، نتیجه قبلی را برمی‌گردانیم
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingResult = await _idempotencyService.GetValueAsync(request.IdempotencyKey, cancellationToken);
            if (!string.IsNullOrEmpty(existingResult))
            {
                // Parse کردن نتیجه قبلی
                try
                {
                    var resultData = JsonSerializer.Deserialize<Dictionary<string, object>>(existingResult);
                    if (resultData?.ContainsKey("OrderId") == true && Guid.TryParse(resultData["OrderId"].ToString(), out var existingOrderId))
                    {
                        _logger.LogInformation("Idempotent request detected. Returning existing order: {OrderId} for key: {Key}", existingOrderId, request.IdempotencyKey);
                        return Result.Success(existingOrderId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing existing idempotency result for key: {Key}", request.IdempotencyKey);
                    // ادامه به پردازش عادی
                }
            }

            // بررسی اینکه آیا در حال پردازش است
            if (await _idempotencyService.IsProcessingAsync(request.IdempotencyKey, cancellationToken))
            {
                _logger.LogWarning("Request with key {Key} is already being processed", request.IdempotencyKey);
                return Result.Failure<Guid>("Request is already being processed. Please wait.");
            }

            // علامت‌گذاری به عنوان در حال پردازش
            await _idempotencyService.MarkAsProcessingAsync(request.IdempotencyKey, TimeSpan.FromMinutes(5), cancellationToken);
        }

        try
        {
            // استفاده از Distributed Lock برای جلوگیری از Race Condition
            var lockKey = $"order:create:{request.CustomerId}";
            await using var lockHandle = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for order creation: {CustomerId}", request.CustomerId);
                return Result.Failure<Guid>("Unable to process order at this time. Please try again.");
            }

            // مرحله 1: رزرو موجودی از Inventory Service (Saga Pattern - Step 1)
            _logger.LogInformation("Reserving inventory for order creation");

            // استفاده از Named HttpClient "DefaultClient" که Policies روی آن اعمال شده است
            var httpClient = _httpClientFactory.CreateClient("DefaultClient");

            // اعتبارسنجی کانفیگ - بدون هیچ مقدار پیش‌فرض/هاردکد
            if (string.IsNullOrWhiteSpace(_inventoryOptions.BaseUrl))
            {
                _logger.LogError("Configuration error: InventoryService:BaseUrl is missing");
                return Result.Failure<Guid>("Configuration error: InventoryService BaseUrl is missing");
            }
            if (string.IsNullOrWhiteSpace(_inventoryOptions.Endpoints?.ReserveProduct) || string.IsNullOrWhiteSpace(_inventoryOptions.Endpoints?.ReleaseProduct))
            {
                _logger.LogError("Configuration error: InventoryService endpoints are missing");
                return Result.Failure<Guid>("Configuration error: InventoryService endpoints are missing");
            }

            if (httpClient.BaseAddress == null)
                httpClient.BaseAddress = new Uri(_inventoryOptions.BaseUrl);

            // اضافه کردن API Key برای Gateway Authentication
            var gatewayApiKey = _gatewayOptions.ApiKey ?? string.Empty;
            if (!string.IsNullOrEmpty(gatewayApiKey))
            {
                httpClient.DefaultRequestHeaders.Add(HeaderNames.GatewayApiKey, gatewayApiKey);
            }

            // رزرو موجودی برای هر محصول - به صورت ترتیبی برای جلوگیری از Deadlock
            // در فشار زیاد، ترتیب مهم است
            var sortedItems = request.Items.OrderBy(i => i.ProductId).ToList();

            foreach (var item in sortedItems)
            {
                // استفاده از Idempotency Key برای جلوگیری از رزرو تکراری
                var idempotencyKey = $"{request.IdempotencyKey ?? Guid.NewGuid().ToString()}-{item.ProductId}";

                var reserveRequest = new
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    IdempotencyKey = idempotencyKey
                };

                var reservePath = _inventoryOptions.Endpoints!.ReserveProduct!.Replace("{id}", item.ProductId.ToString());
                var reserveResponse = await httpClient.PostAsJsonAsync(reservePath, reserveRequest, cancellationToken);

                if (!reserveResponse.IsSuccessStatusCode)
                {
                    var errorContent = await reserveResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Failed to reserve product {ProductId}: {Error}", item.ProductId, errorContent);

                    // Compensation: آزاد کردن تمام موجودی‌های رزرو شده
                    await ReleaseReservedInventory(reservedProducts, httpClient, cancellationToken);

                    return Result.Failure<Guid>($"Insufficient inventory for product {item.ProductName}. Please check available stock.");
                }

                reservedProducts.Add((item.ProductId, item.Quantity));

                _logger.LogInformation("Product {ProductId} reserved successfully: {Quantity}", item.ProductId, item.Quantity);
            }

            // مرحله 2: ایجاد Order (Saga Pattern - Step 2)
            _logger.LogInformation("Creating order after successful inventory reservation");

            var order = BuildOrderFromRequest(request);
            var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);
            orderId = createdOrder.Id;

            _logger.LogInformation("Order created successfully: {OrderId} for customer: {CustomerId}", createdOrder.Id, request.CustomerId);

            // مرحله 3 (Commit) در این فلو انجام نمی‌شود و در مرحله پرداخت/تایید جداگانه انجام خواهد شد

            await SaveIdempotencyResultAsync(request, orderId, cancellationToken);

            return Result.Success(orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer: {CustomerId}", request.CustomerId);

            // Compensation: اگر Order ایجاد نشد، موجودی‌های رزرو شده را آزاد می‌کنیم
            if (!reservedProducts.Any()) 
                return Result.Failure<Guid>($"Error creating order: {ex.Message}");

            var httpClient = _httpClientFactory.CreateClient("DefaultClient");
            if (httpClient.BaseAddress == null)
                httpClient.BaseAddress = new Uri(_inventoryOptions.BaseUrl!);

            var gatewayApiKey = _configuration["Gateway:ApiKey"] ?? string.Empty;
            if (!string.IsNullOrEmpty(gatewayApiKey))
                httpClient.DefaultRequestHeaders.Add("X-Gateway-Api-Key", gatewayApiKey);

            await ReleaseReservedInventory(reservedProducts, httpClient, cancellationToken);
            _logger.LogInformation("Released reserved inventory due to order creation failure");

            return Result.Failure<Guid>($"Error creating order: {ex.Message}");
        }
    }

    private static Order BuildOrderFromRequest(CreateOrderCommand request)
    {
        var order = new Order(request.CustomerId, new List<OrderItem>());
        foreach (var item in request.Items)
        {
            var orderItem = new OrderItem(Guid.Empty, item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
            order.AddItem(orderItem);
        }
        return order;
    }

    private async Task SaveIdempotencyResultAsync(CreateOrderCommand request, Guid orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.IdempotencyKey))
            return;

        var resultJson = JsonSerializer.Serialize(new { OrderId = orderId, CreatedAt = DateTime.UtcNow });
        await _idempotencyService.SetValueAsync(request.IdempotencyKey, resultJson, TimeSpan.FromHours(24), cancellationToken);
        _logger.LogInformation("Idempotency result stored for key: {Key}, OrderId: {OrderId}", request.IdempotencyKey, orderId);
    }

    /// <summary>
    /// آزاد کردن موجودی‌های رزرو شده (Compensation Pattern)
    /// </summary>
    private async Task ReleaseReservedInventory(List<(Guid ProductId, int Quantity)> reservedProducts, HttpClient httpClient, CancellationToken cancellationToken)
    {
        foreach (var (productId, quantity) in reservedProducts)
        {
            try
            {
                _logger.LogInformation("Releasing {Quantity} of product {ProductId}", quantity, productId);

                var releasePath = _inventoryOptions.Endpoints!.ReleaseProduct!.Replace("{id}", productId.ToString());
                var response = await httpClient.PostAsJsonAsync(releasePath, new { Quantity = quantity }, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Product {ProductId} released successfully: {Quantity}", productId, quantity);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Failed to release product {ProductId}: {Error}", productId, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing reserved inventory for product {ProductId}", productId);
            }
        }
    }
}

