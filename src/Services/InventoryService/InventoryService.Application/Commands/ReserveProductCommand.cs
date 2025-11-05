using MediatR;
using Shared.Common;

namespace InventoryService.Application.Commands;

/// <summary>
/// کامند رزرو موجودی محصول
/// این کامند برای جلوگیری از Race Condition در زمان رزرو موجودی استفاده می‌شود
/// </summary>
public class ReserveProductCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    
    // Idempotency Key برای جلوگیری از درخواست‌های تکراری
    // در فشار زیاد، ممکن است درخواست چندین بار ارسال شود
    public string? IdempotencyKey { get; set; }
}

