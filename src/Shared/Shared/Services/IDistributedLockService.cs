namespace Shared.Services;

/// <summary>
/// سرویس Distributed Lock برای جلوگیری از Race Conditions
/// از Redis برای پیاده‌سازی Distributed Lock استفاده می‌کند
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// دریافت قفل برای یک کلید مشخص
    /// </summary>
    /// <param name="key">کلید Lock</param>
    /// <param name="expirationTime">زمان انقضای Lock (پیش‌فرض 30 ثانیه)</param>
    /// <param name="cancellationToken">توکن لغو</param>
    /// <returns>IDisposable که با Dispose شدن، Lock آزاد می‌شود</returns>
    Task<IAsyncDisposable?> AcquireLockAsync(string key, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// بررسی اینکه آیا Lock موجود است یا خیر
    /// </summary>
    Task<bool> IsLockedAsync(string key, CancellationToken cancellationToken = default);
}

