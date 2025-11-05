using MediatR;
using Shared.Common;

namespace InventoryService.Application.Commands;

/// <summary>
/// کامند ایجاد محصول جدید
/// </summary>
public class CreateProductCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InitialStock { get; set; }
}

