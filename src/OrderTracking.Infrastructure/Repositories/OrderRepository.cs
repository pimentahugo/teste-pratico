using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Data.SqlServer;

namespace OrderTracking.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
	private readonly OrderTrackingContext _context;

	public OrderRepository(OrderTrackingContext context)
	{
		_context = context;
	}

	public async Task<Order?> GetByIdAsync(Guid id)
	{
		return await _context.Orders.FindAsync(id);
	}

	public async Task AddAsync(Order order)
	{
		await _context.Orders.AddAsync(order);
		await _context.SaveChangesAsync();
	}
}