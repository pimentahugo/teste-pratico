using MongoDB.Driver;
using OrderTracking.Domain.Entities;
using OrderTracking.Infrastructure.Data.Mongo.Models;

namespace OrderTracking.Infrastructure.Services.Cache;
public interface IOrderCacheService
{
	Task<Order?> GetOrderAsync(Guid orderId);
	Task SetOrderAsync(Order order);
	Task RemoveOrderAsync(Guid orderId);
}

public class OrderCacheService : IOrderCacheService
{
	private readonly IMongoCollection<OrderCacheModel> _collection;

	public OrderCacheService(IMongoCollection<OrderCacheModel> collection)
	{
		_collection = collection;
	}

	public async Task<Order?> GetOrderAsync(Guid orderId)
	{
		var order = await _collection
							.Find(p => p.Id == orderId)
							.FirstOrDefaultAsync();

		if (order?.ExpiresAt > DateTime.Now)
		{
			return order.ToDomainEntity();
		}

		return null;
	}

	public async Task SetOrderAsync(Order order)
	{
		var cacheModel = order.ToCacheModel();

		await _collection.ReplaceOneAsync(x => x.Id == order.Id, cacheModel, new ReplaceOptions { IsUpsert = true });
	}

	public async Task RemoveOrderAsync(Guid orderId)
	{
		await _collection.DeleteOneAsync(p => p.Id == orderId);
	}
}