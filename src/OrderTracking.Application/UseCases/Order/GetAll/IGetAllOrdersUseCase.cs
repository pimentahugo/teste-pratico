using OrderTracking.Application.Responses;
using OrderTracking.Domain.Repositories;
using OrderTracking.Shared.Results;

namespace OrderTracking.Application.UseCases.Order.GetAll;
public interface IGetAllOrdersUseCase
{
	Task<Result<IEnumerable<OrderResponse>>> ExecuteAsync();
}

public class GetAllOrdersUseCase : IGetAllOrdersUseCase
{
	private readonly IOrderRepository _orderRepository;
	public GetAllOrdersUseCase(IOrderRepository orderRepository)
	{
		_orderRepository = orderRepository;
	}
	public async Task<Result<IEnumerable<OrderResponse>>> ExecuteAsync()
	{
		var orders = await _orderRepository.GetAllAsync();

		var orderResponses = orders.Select(order => new OrderResponse(order.Id, order.Cliente, order.Valor, order.DataPedido, order.DataCriacao));

		return Result<IEnumerable<OrderResponse>>.Ok(orderResponses);
	}
}