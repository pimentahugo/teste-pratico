using Microsoft.EntityFrameworkCore;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Data.SqlServer;
using OrderTracking.Infrastructure.Services.Cache;

namespace OrderTracking.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
	private readonly OrderTrackingContext _context;
	private readonly IOrderCacheService _cacheService;

	public OrderRepository(OrderTrackingContext context, IOrderCacheService cacheService)
	{
		_context = context;
		_cacheService = cacheService;
	}

	public async Task<IEnumerable<Order>> GetAllAsync()
	{
		return await _context.Orders.ToListAsync();
	}

	public async Task<Order?> GetByIdAsync(Guid id)
	{
		var orderCached = await _cacheService.GetOrderAsync(id);

		if (orderCached is not null)
			return orderCached;

		var order = await _context.Orders.FindAsync(id);

		if(order is not null)
			await _cacheService.SetOrderAsync(order);

		return order;
	}

	public async Task AddAsync(Order order)
	{
		await _context.Orders.AddAsync(order);
		await _context.SaveChangesAsync();
	}
}