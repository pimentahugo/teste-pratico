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

	public OrderEventHandler(IOrderRepository repository)
	{
		_repository = repository;
	}

	public async Task HandleAsync(PedidoCriadoEvent orderEvent, CancellationToken cancellationToken = default)
	{
		var existingOrder = await _repository.GetByIdAsync(orderEvent.PedidoId);

		if (existingOrder is not null)
			return;

		var order = Order.Criar(orderEvent.PedidoId, orderEvent.Cliente, orderEvent.Valor, orderEvent.DataPedido);

		await _repository.AddAsync(order);
	}
}