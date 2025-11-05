namespace InventoryService.Domain.Entities;

/// <summary>
/// موجودیت محصول در انبار
/// این موجودیت شامل اطلاعات محصول و موجودی آن است
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Version برای Optimistic Concurrency Control
    // این فیلد برای جلوگیری از Race Condition در فشار زیاد استفاده می‌شود
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // Available Quantity = StockQuantity - ReservedQuantity
    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    // Constructor برای EF Core
    private Product() { }

    public Product(string name, string description, decimal price, int initialStock)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Price = price >= 0 ? price : throw new ArgumentException("Price cannot be negative", nameof(price));
        StockQuantity = initialStock >= 0 ? initialStock : throw new ArgumentException("Stock cannot be negative", nameof(initialStock));
        ReservedQuantity = 0;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// رزرو موجودی برای سفارش
    /// این متد از Race Condition جلوگیری می‌کند
    /// </summary>
    public void ReserveQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException($"Insufficient available quantity. Available: {AvailableQuantity}, Requested: {quantity}");

        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// آزاد کردن موجودی رزرو شده
    /// </summary>
    public void ReleaseReservedQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (ReservedQuantity < quantity)
            throw new InvalidOperationException($"Cannot release more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// کاهش موجودی (برای زمان تحویل سفارش)
    /// </summary>
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (ReservedQuantity < quantity)
            throw new InvalidOperationException($"Cannot decrease more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

        StockQuantity -= quantity;
        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// افزایش موجودی
    /// </summary>
    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// به‌روزرسانی اطلاعات محصول
    /// </summary>
    public void Update(string name, string description, decimal price)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Price = price >= 0 ? price : throw new ArgumentException("Price cannot be negative", nameof(price));
        UpdatedAt = DateTime.UtcNow;
    }
}

