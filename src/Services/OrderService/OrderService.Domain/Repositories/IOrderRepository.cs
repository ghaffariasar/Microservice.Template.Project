namespace OrderService.Domain.Repositories;

using Entities;

/// <summary>
/// اینترفیس Repository برای سفارش‌ها
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}

