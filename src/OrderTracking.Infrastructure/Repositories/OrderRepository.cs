using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Data.SqlServer;
using OrderTracking.Infrastructure.Services.Cache;
using Serilog.Core;

namespace OrderTracking.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
	private readonly OrderTrackingContext _context;
	private readonly IOrderCacheService _cacheService;
	private readonly ILogger<OrderRepository> _logger;

	public OrderRepository(
		OrderTrackingContext context,
		IOrderCacheService cacheService,
		ILogger<OrderRepository> logger)
	{
		_context = context;
		_cacheService = cacheService;
		_logger = logger;
	}

	public async Task<IEnumerable<Order>> GetAllAsync()
	{
		_logger.LogInformation("Buscando todos os pedidos no banco de dados");

		var orders = await _context.Orders.ToListAsync();

		_logger.LogInformation(
			"Pedidos recuperados com sucesso. Total: {OrderCount}",
			orders.Count);

		return orders;
	}

	public async Task<Order?> GetByIdAsync(Guid id)
	{
		_logger.LogDebug(
				"Buscando pedido {OrderId} no cache",
				id);

		var orderCached = await _cacheService.GetOrderAsync(id);

		if (orderCached is not null)
		{
			_logger.LogInformation(
					"Pedido {OrderId} encontrado no cache",
					id);
			return orderCached;
		}

		_logger.LogDebug(
				"Pedido {OrderId} não encontrado no cache (cache miss). Buscando no banco de dados",
				id);

		var order = await _context.Orders.FindAsync(id);

		if(order is not null)
		{
			_logger.LogInformation(
					"Pedido {OrderId} encontrado no banco de dados. Salvando no cache",
					id);
			await _cacheService.SetOrderAsync(order);
		}

		return order;
	}

	public async Task AddAsync(Order order)
	{
		_logger.LogInformation(
				"Salvando pedido {OrderId} no banco de dados. Cliente: {Cliente}, Valor: {Valor}",
				order.Id,
				order.Cliente,
				order.Valor);

		await _context.Orders.AddAsync(order);
		await _context.SaveChangesAsync();

		_logger.LogInformation(
				"Pedido {OrderId} salvo com sucesso no banco de dados",
				order.Id);
	}
}