using OrderService.Domain.Entities;

namespace OrderService.Application.DTOs;

/// <summary>
/// DTO برای سفارش - برای انتقال داده بین لایه‌ها
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO برای آیتم سفارش
/// </summary>
public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

