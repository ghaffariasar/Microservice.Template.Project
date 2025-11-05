namespace OrderService.Domain.Entities;

/// <summary>
/// آیتم سفارش - شامل اطلاعات محصول و تعداد سفارش شده
/// </summary>
public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; internal set; } // internal برای تنظیم از طریق EF Core و Domain
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Constructor برای EF Core
    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        UnitPrice = unitPrice > 0 ? unitPrice : throw new ArgumentException("UnitPrice must be greater than 0", nameof(unitPrice));
    }

    /// <summary>
    /// تغییر تعداد آیتم
    /// </summary>
    public void ChangeQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(newQuantity));

        Quantity = newQuantity;
    }
}

