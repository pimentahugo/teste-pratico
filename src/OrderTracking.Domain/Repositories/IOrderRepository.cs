using OrderTracking.Domain.Entities;

namespace OrderTracking.Domain.Repositories;
public interface IOrderRepository
{
	Task<Order?> GetByIdAsync(Guid id);
	Task AddAsync(Order order);
}