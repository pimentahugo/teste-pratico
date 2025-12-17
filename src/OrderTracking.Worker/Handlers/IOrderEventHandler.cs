using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Messaging.Events;

namespace OrderTracking.Worker.Handlers;
public interface IOrderEventHandler
{
	Task HandleAsync(PedidoCriadoEvent orderEvent, CancellationToken cancellationToken = default);
}

public class OrderEventHandler : IOrderEventHandler
{
	private readonly IOrderRepository _repository;
	private readonly ILogger<OrderEventHandler> _logger;

	public OrderEventHandler(
		IOrderRepository repository,
		ILogger<OrderEventHandler> logger)
	{
		_repository = repository;
		_logger = logger;
	}

	public async Task HandleAsync(PedidoCriadoEvent orderEvent, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(
			"Processando evento de pedido criado. OrderId: {OrderId}, Cliente: {Cliente}, Valor: {Valor}",
			orderEvent.PedidoId,
			orderEvent.Cliente,
			orderEvent.Valor);

		var existingOrder = await _repository.GetByIdAsync(orderEvent.PedidoId);

		if (existingOrder is not null)
		{
			_logger.LogWarning(
					"Pedido {OrderId} já foi processado anteriormente. Ignorando evento duplicado",
					orderEvent.PedidoId);
			return;
		}

		var order = Order.Criar(orderEvent.PedidoId, orderEvent.Cliente, orderEvent.Valor, orderEvent.DataPedido);

		_logger.LogInformation(
				"Salvando pedido {OrderId} no banco de dados via Worker",
				orderEvent.PedidoId);
		
		await _repository.AddAsync(order);

		_logger.LogInformation(
				"Pedido {OrderId} processado e salvo com sucesso. Cliente: {Cliente}, Valor: {Valor:C}",
				orderEvent.PedidoId,
				orderEvent.Cliente,
				orderEvent.Valor);
	}
}