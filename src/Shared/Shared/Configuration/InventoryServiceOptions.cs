namespace Shared.Configuration;

/// <summary>
/// تنظیمات سرویس موجودی برای استفاده در ارتباطات بین‌سرویسی
/// </summary>
public class InventoryServiceOptions
{
    /// <summary>
    /// آدرس پایه سرویس موجودی (مثلاً http://localhost:5002)
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// مسیرهای API سرویس موجودی
    /// </summary>
    public InventoryEndpoints Endpoints { get; set; } = new();
}

public class InventoryEndpoints
{
    /// <summary>
    /// مسیر رزرو محصول. از {id} به عنوان placeholder شناسه محصول استفاده کنید.
    /// مثال: /api/products/{id}/reserve
    /// </summary>
    public string? ReserveProduct { get; set; }

    /// <summary>
    /// مسیر آزادسازی رزرو. مثال: /api/products/{id}/release
    /// </summary>
    public string? ReleaseProduct { get; set; }

    /// <summary>
    /// مسیر تایید نهایی (commit) موجودی. مثال: /api/products/{id}/commit
    /// </summary>
    public string? CommitProduct { get; set; }
}


