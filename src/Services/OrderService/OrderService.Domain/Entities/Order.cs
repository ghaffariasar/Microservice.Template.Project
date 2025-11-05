namespace OrderService.Domain.Entities;

/// <summary>
/// موجودیت سفارش
/// این موجودیت شامل اطلاعات سفارش و وضعیت آن است
/// </summary>
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Constructor برای EF Core
    private Order() { }

    public Order(Guid customerId, List<OrderItem> items)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        _items = items ?? throw new ArgumentNullException(nameof(items));

        // تنظیم OrderId برای تمام آیتم‌ها
        foreach (var item in _items)
        {
            SetOrderIdForItem(item);
        }
        TotalAmount = _items.Sum(i => i.UnitPrice * i.Quantity);
    }

    /// <summary>
    /// اضافه کردن آیتم به سفارش
    /// </summary>
    public void AddItem(OrderItem item)
    {
        SetOrderIdForItem(item);
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.UnitPrice * i.Quantity);
    }

    /// <summary>
    /// تنظیم OrderId برای آیتم
    /// </summary>
    private void SetOrderIdForItem(OrderItem item)
    {
        // استفاده از internal setter
        item.OrderId = Id;
    }

    /// <summary>
    /// تغییر وضعیت سفارش
    /// </summary>
    public void ChangeStatus(OrderStatus newStatus)
    {
        if (Status == OrderStatus.Cancelled || Status == OrderStatus.Completed)
            throw new InvalidOperationException($"Cannot change status from {Status} to {newStatus}");

        Status = newStatus;
    }

    /// <summary>
    /// لغو سفارش
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed order");

        Status = OrderStatus.Cancelled;
    }
}

/// <summary>
/// وضعیت‌های سفارش
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Completed = 5,
    Cancelled = 6
}

