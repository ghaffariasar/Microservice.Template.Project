using MediatR;
using Shared.Common;

namespace InventoryService.Application.Commands;

public class UpdateProductCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}


