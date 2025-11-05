using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using Shared.Extensions;
using Shared.Configuration;
using Polly;
using Polly.Extensions.Http;
using Shared.Services;

namespace OrderService.Infrastructure;

/// <summary>
/// کلاس برای ثبت سرویس‌های Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // تنظیمات دیتابیس - InMemory به صورت پیش‌فرض
        var useInMemory = configuration.GetValue<bool>("Database:UseInMemory", true);

        if (useInMemory)
        {
            services.AddDbContext<OrderDbContext>(options => options.UseInMemoryDatabase("OrderDb"));
        }
        else
        {
            // استفاده از SQL Server در صورت نیاز
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(connectionString));
        }

        // ثبت Repository
        services.AddScoped<IOrderRepository, OrderRepository>();

        // تنظیمات Distributed Cache و Distributed Lock
        // پشتیبانی از Redis, Memory, و SQL Server Cache
        // Cache Provider از Configuration خوانده می‌شود (پیش‌فرض: Redis)
        services.AddDistributedCacheAndLock(configuration);

        // ثبت Idempotency Service (از IDistributedCache استفاده می‌کند)
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // Bind & validate InventoryService options (fail fast if invalid)
        services.AddOptions<InventoryServiceOptions>()
            .Bind(configuration.GetSection("InventoryService"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "InventoryService:BaseUrl is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoints?.ReserveProduct), "InventoryService:Endpoints:ReserveProduct is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoints?.ReleaseProduct), "InventoryService:Endpoints:ReleaseProduct is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoints?.CommitProduct), "InventoryService:Endpoints:CommitProduct is required")
            .ValidateOnStart();

        // Bind & validate Gateway options
        services.AddOptions<GatewayOptions>()
            .Bind(configuration.GetSection("Gateway"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Gateway:ApiKey is required")
            .ValidateOnStart();

        // ثبت Named HttpClient پیش‌فرض با Policies (برای استفاده عمومی)
        services.AddHttpClient("DefaultClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    /// <summary>
    /// Retry Policy برای HTTP Client
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry (در صورت نیاز)
                });
    }

    /// <summary>
    /// Circuit Breaker Policy برای HTTP Client
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}

