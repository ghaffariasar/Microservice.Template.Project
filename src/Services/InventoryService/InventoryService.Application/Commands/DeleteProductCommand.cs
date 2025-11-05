using MediatR;
using Shared.Common;

namespace InventoryService.Application.Commands;

public class DeleteProductCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
}


