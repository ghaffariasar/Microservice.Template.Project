namespace Shared.Services;

/// <summary>
/// این سرویس برای جلوگیری از پردازش درخواست‌های تکراری   
/// این سرویس کلید ها را در ردیس ذخیره می‌کند و TTL دارد
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// بررسی اینکه آیا Key قبلاً پردازش شده است یا خیر
    /// </summary>
    /// <param name="key">Idempotency Key</param>
    /// <param name="cancellationToken">توکن لغو</param>
    /// <returns>اگر Key موجود باشد، مقدار قبلی را برمی‌گرداند. در غیر این صورت null</returns>
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// ذخیره Key و مقدار آن (نتیجه عملیات)
    /// </summary>
    /// <param name="key">Idempotency Key</param>
    /// <param name="value">نتیجه عملیات (JSON)</param>
    /// <param name="expirationTime">زمان انقضا (پیش‌فرض 24 ساعت)</param>
    /// <param name="cancellationToken">توکن لغو</param>
    Task SetValueAsync(string key, string value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// بررسی اینکه آیا Key در حال پردازش است یا خیر (برای جلوگیری از پردازش همزمان)
    /// </summary>
    Task<bool> IsProcessingAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// علامت‌گذاری Key به عنوان در حال پردازش
    /// </summary>
    Task MarkAsProcessingAsync(string key, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);
}

