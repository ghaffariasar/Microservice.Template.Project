using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InventoryService.Domain.Repositories;
using InventoryService.Infrastructure.Data;
using InventoryService.Infrastructure.Repositories;
using Shared.Extensions;
using Shared.Configuration;
using Shared.Services;

namespace InventoryService.Infrastructure;

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
            services.AddDbContext<InventoryDbContext>(options => options.UseInMemoryDatabase("InventoryDb"));
        else
        {
            // استفاده از SQL Server در صورت نیاز
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(connectionString));
        }

        // ثبت Repository
        services.AddScoped<IProductRepository, ProductRepository>();

        // تنظیمات Distributed Cache و Distributed Lock
        // پشتیبانی از Redis, Memory, و SQL Server Cache
        // Cache Provider از Configuration خوانده می‌شود (پیش‌فرض: Redis)
        services.AddDistributedCacheAndLock(configuration);

        // ثبت Idempotency Service (از IDistributedCache استفاده می‌کند)
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // Bind & validate Gateway options for middleware
        services.AddOptions<GatewayOptions>()
            .Bind(configuration.GetSection("Gateway"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Gateway:ApiKey is required")
            .ValidateOnStart();

        return services;
    }
}

