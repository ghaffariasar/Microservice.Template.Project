using Microsoft.Extensions.Logging;
using StackExchange.Redis;


namespace Shared.Services;

/// <summary>
/// پیاده‌سازی Distributed Lock با استفاده از Redis
/// این سرویس برای جلوگیری از Race Conditions در محیط‌های توزیع شده استفاده می‌شود
/// </summary>
public class DistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<DistributedLockService> _logger;
    private const string LockPrefix = "lock:";

    public DistributedLockService(IConnectionMultiplexer redis, ILogger<DistributedLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IAsyncDisposable?> AcquireLockAsync(string key, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        expirationTime ??= TimeSpan.FromSeconds(30);
        var lockKey = LockPrefix + key;
        var lockValue = Guid.NewGuid().ToString();
        var database = _redis.GetDatabase();

        // تلاش برای دریافت قفل با استفاده از SET NX EX
        var acquired = await database.StringSetAsync(lockKey, lockValue, expirationTime, When.NotExists).ConfigureAwait(false);

        if (acquired)
        {
            _logger.LogInformation("Lock acquired for key: {Key}", key);
            return new RedisLock(database, lockKey, lockValue, _logger);
        }

        _logger.LogWarning("Failed to acquire lock for key: {Key}", key);
        return null;
    }

    public async Task<bool> IsLockedAsync(string key, CancellationToken cancellationToken = default)
    {
        var lockKey = LockPrefix + key;
        var database = _redis.GetDatabase();
        return await database.KeyExistsAsync(lockKey);
    }

    /// <summary>
    /// کلاس داخلی برای مدیریت Redis Lock
    /// </summary>
    private class RedisLock : IAsyncDisposable
    {
        private readonly IDatabase _database;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly ILogger _logger;
        private bool _disposed;

        public RedisLock(IDatabase database, string lockKey, string lockValue, ILogger logger)
        {
            _database = database;
            _lockKey = lockKey;
            _lockValue = lockValue;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            const string script = @"if redis.call('get', KEYS[1]) == ARGV[1] then
                                        return redis.call('del', KEYS[1])
                                   else
                                        return 0
                                   end";

            try
            {
                await _database.ScriptEvaluateAsync(script, new RedisKey[] { _lockKey }, new RedisValue[] { _lockValue });
                _logger.LogInformation("Lock released for key: {Key}", _lockKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock for key: {Key}", _lockKey);
            }

            _disposed = true;
        }


    }
}

