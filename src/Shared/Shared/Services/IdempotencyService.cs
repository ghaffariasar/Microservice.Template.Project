using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Shared.Services;

/// <summary>
/// پیاده‌سازی Idempotency Service با استفاده از Redis Distributed Cache
/// کلید ها بصورت زمان دار ذخیره می‌شوند و به صورت خودکار حذف می‌شوند
/// </summary>
public class IdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<IdempotencyService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(24);
    private const string ProcessingPrefix = "idempotency:processing:";
    private const string ResultPrefix = "idempotency:result:";

    public IdempotencyService(IDistributedCache cache, ILogger<IdempotencyService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = ResultPrefix + key;
            var value = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(value))
            {
                _logger.LogInformation("Idempotency key found: {Key}", key);
                return value;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting idempotency value for key: {Key}", key);
            return null; // در صورت خطا، null برمی‌گردانیم تا پردازش ادامه پیدا کند
        }
    }

    public async Task SetValueAsync(string key, string value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = ResultPrefix + key;
            var expiration = expirationTime ?? _defaultExpiration;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(cacheKey, value, options, cancellationToken);
            _logger.LogInformation("Idempotency key stored: {Key}, Expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting idempotency value for key: {Key}", key);
            // خطا را Log می‌کنیم اما Exception نمی‌اندازیم تا عملیات اصلی ادامه پیدا کند
        }
    }

    public async Task<bool> IsProcessingAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = ProcessingPrefix + key;
            var value = await _cache.GetStringAsync(cacheKey, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking processing status for key: {Key}", key);
            return false;
        }
    }

    public async Task MarkAsProcessingAsync(string key, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = ProcessingPrefix + key;
            var expiration = expirationTime ?? TimeSpan.FromMinutes(5); // 5 دقیقه برای Processing

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(cacheKey, "processing", options, cancellationToken);
            _logger.LogInformation("Idempotency key marked as processing: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking idempotency key as processing: {Key}", key);
        }
    }
}

