using OrderTracking.Application.Responses;
using OrderTracking.Domain.Repositories;
using OrderTracking.Shared.Messages;
using OrderTracking.Shared.Results;

namespace OrderTracking.Application.UseCases.Order.GetById;
public interface IOrderGetByIdUseCase
{
	Task<Result<OrderResponse>> ExecuteAsync(Guid orderId);
}

public class OrderGetByIdUseCase : IOrderGetByIdUseCase
{
	private readonly IOrderRepository _orderRepository;
	public OrderGetByIdUseCase(IOrderRepository orderRepository)
	{
		_orderRepository = orderRepository;
	}
	public async Task<Result<OrderResponse>> ExecuteAsync(Guid orderId)
	{
		var order = await _orderRepository.GetByIdAsync(orderId);

		if (order is null)
		{
			return Result<OrderResponse>.Fail(ValidationMessages.Order_NotFound);
		}

		var response = new OrderResponse(order.Id, order.Cliente, order.Valor, order.DataPedido, order.DataCriacao);

		return Result<OrderResponse>.Ok(response);
	}
}