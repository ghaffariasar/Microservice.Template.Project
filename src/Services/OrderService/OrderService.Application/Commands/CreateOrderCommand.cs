using MediatR;
using Shared.Common;

namespace OrderService.Application.Commands;

/// <summary>
/// کامند ایجاد سفارش جدید
/// این کامند از الگوی CQRS استفاده می‌کند
/// </summary>
public class CreateOrderCommand : IRequest<Result<Guid>>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    
    // Idempotency Key برای جلوگیری از ایجاد سفارش تکراری
    // در فشار زیاد، ممکن است درخواست چندین بار ارسال شود
    public string? IdempotencyKey { get; set; }
}

/// <summary>
/// DTO برای آیتم‌های سفارش
/// </summary>
public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

