using FluentValidation;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Infrastructure.Messaging.Events;
using OrderTracking.Shared.Results;

namespace OrderTracking.Application.UseCases.Order.Create;

public interface ICreateOrderUseCase
{
	Task<Result<bool>> ExecuteAsync(CreateOrderRequest request);
}

public class CreateOrderUseCase : ICreateOrderUseCase
{
	private readonly IValidator<CreateOrderRequest> _validator;
	private readonly IMessagePublisher _publisher;
	public CreateOrderUseCase(IValidator<CreateOrderRequest> validator, IMessagePublisher publisher)
	{
		_validator = validator;
		_publisher = publisher;
	}

	public async Task<Result<bool>> ExecuteAsync(CreateOrderRequest request)
	{
		var validationResult = await _validator.ValidateAsync(request);

		if(validationResult.IsValid is false)
		{
			var errors = validationResult.Errors.Select(e => e.ErrorMessage);
			return Result<bool>.Fail(errors, "Erro de validação.");
		}

		var orderEvent = new PedidoCriadoEvent(request.Id, request.Cliente, request.Valor, request.DataPedido);

		await _publisher.PublishAsync(orderEvent);

		return Result<bool>.Ok(true);
	}
}