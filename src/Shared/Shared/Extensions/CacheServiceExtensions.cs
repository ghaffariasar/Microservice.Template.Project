using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Services;
using StackExchange.Redis;

namespace Shared.Extensions;

/// <summary>
/// Extension Methods برای تنظیم Distributed Cache و Distributed Lock
/// پشتیبانی از Redis، Memory Cache و SQL Server Cache
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Cache Provider Types
    /// </summary>
    public enum CacheProviderType
    {
        Redis,
        Memory,
        SqlServer
    }

    /// <summary>
    /// افزودن Distributed Cache و Distributed Lock بر اساس Configuration
    /// </summary>
    public static IServiceCollection AddDistributedCacheAndLock(this IServiceCollection services, IConfiguration configuration)
    {
        // خواندن Cache Provider از Configuration
        var cacheProvider = configuration.GetValue<string>("Cache:Provider") ?? "Redis";

        if (!Enum.TryParse<CacheProviderType>(cacheProvider, true, out var providerType))
        {
            providerType = CacheProviderType.Redis; // پیش‌فرض
        }

        // ثبت Distributed Cache بر اساس Provider
        switch (providerType)
        {
            case CacheProviderType.Redis:

                AddRedisCache(services, configuration);

                AddRedisDistributedLock(services, configuration);

                break;

            case CacheProviderType.Memory:

                AddMemoryCache(services);

                // Distributed Lock با Memory Cache معنا ندارد
                // از یک NoOp Implementation استفاده می‌کنیم
                AddNoOpDistributedLock(services);

                break;

            case CacheProviderType.SqlServer:

                AddSqlServerCache(services, configuration);

                // Distributed Lock با SQL Server Cache نیز معنا ندارد
                AddNoOpDistributedLock(services);

                break;
        }

        return services;
    }

    /// <summary>
    /// افزودن Redis Cache
    /// </summary>
    private static void AddRedisCache(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? configuration["Cache:Redis:ConnectionString"];
        if (redisConnectionString == null)
            throw new InvalidOperationException("Redis Cache requires 'Redis:ConnectionString' in ConnectionStrings");

        // ثبت StackExchangeRedisCache برای IDistributedCache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        // ثبت IConnectionMultiplexer برای DistributedLockService
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
    }

    /// <summary>
    /// افزودن Memory Cache
    /// </summary>
    private static void AddMemoryCache(IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
    }

    /// <summary>
    /// افزودن SQL Server Cache
    /// </summary>
    private static void AddSqlServerCache(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("SQL Server Cache requires 'DefaultConnection' in ConnectionStrings");

        var schemaName = configuration["Cache:SqlServer:SchemaName"] ?? "dbo";
        var tableName = configuration["Cache:SqlServer:TableName"] ?? "Cache";

        services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = connectionString;
            options.SchemaName = schemaName;
            options.TableName = tableName;
        });
    }

    /// <summary>
    /// افزودن Redis Distributed Lock
    /// </summary>
    private static void AddRedisDistributedLock(IServiceCollection services, IConfiguration configuration)
    {
        // DistributedLockService از IConnectionMultiplexer استفاده می‌کند
        // که در AddRedisCache ثبت شده است
        services.AddScoped<IDistributedLockService, DistributedLockService>();
    }

    /// <summary>
    /// افزودن NoOp Distributed Lock (برای Memory/SQL Server Cache)
    /// </summary>
    private static void AddNoOpDistributedLock(IServiceCollection services)
    {
        services.AddScoped<IDistributedLockService, NoOpDistributedLockService>();
    }
}

/// <summary>
/// پیاده‌سازی NoOp برای Distributed Lock
/// زمانی استفاده می‌شود که Cache Provider از Redis نیست
/// </summary>
internal class NoOpDistributedLockService : IDistributedLockService
{
    private readonly ILogger<NoOpDistributedLockService> _logger;

    public NoOpDistributedLockService(ILogger<NoOpDistributedLockService> logger)
    {
        _logger = logger;
    }

    public Task<IAsyncDisposable?> AcquireLockAsync(string key, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Distributed Lock requested but not supported with current Cache Provider. " +
            "Lock will be granted but not actually enforced. Use Redis for proper distributed locks. Key: {Key}",
            key);

        // یک NoOp Lock برمی‌گردانیم
        return Task.FromResult<IAsyncDisposable?>(new NoOpLock());
    }

    public Task<bool> IsLockedAsync(string key, CancellationToken cancellationToken = default)
    {
        // همیشه false برمی‌گردانیم (Lock وجود ندارد)
        return Task.FromResult(false);
    }

    private class NoOpLock : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {           
            // TODO release managed resources here
            await Task.CompletedTask;
        }
    }
}

