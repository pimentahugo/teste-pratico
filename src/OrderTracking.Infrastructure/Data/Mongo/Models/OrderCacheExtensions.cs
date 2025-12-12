using OrderTracking.Domain.Entities;

namespace OrderTracking.Infrastructure.Data.Mongo.Models;
public static class OrderCacheExtensions
{
	public static OrderCacheModel ToCacheModel(this Order order)
	{
		return new OrderCacheModel
		{
			Id = order.Id,
			Cliente = order.Cliente,
			Valor = order.Valor,
			DataPedido = order.DataPedido,
			DataCriacao = order.DataCriacao,
			CachedAt = DateTime.Now,
			ExpiresAt = DateTime.Now.AddMinutes(10)
		};
	}

	public static Order ToDomainEntity(this OrderCacheModel cacheModel)
	{
		return Order.FromCache(
				cacheModel.Id, 
				cacheModel.Cliente, 
				cacheModel.Valor, 
				cacheModel.DataPedido, 
				cacheModel.DataCriacao);
	}
}