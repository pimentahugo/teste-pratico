using Microsoft.Extensions.Logging;
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
	private readonly ILogger<OrderCacheService> _logger;

	public OrderCacheService(
		IMongoCollection<OrderCacheModel> collection,
		ILogger<OrderCacheService> logger)
	{
		_collection = collection;
		_logger = logger;
	}

	public async Task<Order?> GetOrderAsync(Guid orderId)
	{
		_logger.LogDebug(
				"Buscando pedido {OrderId} no cache MongoDB",
				orderId);

		var order = await _collection
							.Find(p => p.Id == orderId)
							.FirstOrDefaultAsync();

		if (order?.ExpiresAt > DateTime.Now)
		{
			_logger.LogDebug(
				"Pedido {OrderId} encontrado no cache e ainda válido",
				orderId);

			return order.ToDomainEntity();
		}

		return null;
	}

	public async Task SetOrderAsync(Order order)
	{
		_logger.LogDebug(
			"Salvando pedido {OrderId} no cache MongoDB",
			order.Id);

		var cacheModel = order.ToCacheModel();

		await _collection.ReplaceOneAsync(x => x.Id == order.Id, cacheModel, new ReplaceOptions { IsUpsert = true });

		_logger.LogDebug(
			   "Pedido {OrderId} salvo no cache com sucesso. ExpiresAt: {ExpiresAt}",
			   order.Id,
			   cacheModel.ExpiresAt);
	}

	public async Task RemoveOrderAsync(Guid orderId)
	{
		_logger.LogDebug(
			  "Removendo pedido {OrderId} do cache MongoDB",
			  orderId);

		await _collection.DeleteOneAsync(p => p.Id == orderId);

		_logger.LogDebug(
				"Pedido {OrderId} removido do cache com sucesso",
				orderId);
	}
}