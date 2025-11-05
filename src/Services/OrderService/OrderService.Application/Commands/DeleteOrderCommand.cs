using MediatR;
using Shared.Common;

namespace OrderService.Application.Commands;

public class DeleteOrderCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
}


