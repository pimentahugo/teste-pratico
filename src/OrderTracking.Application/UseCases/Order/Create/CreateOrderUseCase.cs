using FluentValidation;
using Microsoft.Extensions.Logging;
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
	private readonly ILogger<CreateOrderUseCase> _logger;
	public CreateOrderUseCase(IValidator<CreateOrderRequest> validator, IMessagePublisher publisher, ILogger<CreateOrderUseCase> logger)
	{
		_validator = validator;
		_publisher = publisher;
		_logger = logger;
	}

	public async Task<Result<bool>> ExecuteAsync(CreateOrderRequest request)
	{
		_logger.LogInformation(
			   "Iniciando criação do pedido {OrderId} para o cliente {Cliente}",
			   request.Id,
			   request.Cliente
		   );

		var validationResult = await _validator.ValidateAsync(request);

		if (!validationResult.IsValid)
		{
			var errors = validationResult.Errors.Select(e => e.ErrorMessage);

			_logger.LogInformation(
				"Falha na validação do pedido {OrderId}. Erros: {ValidationErrors}",
				request.Id,
				string.Join(", ", errors));

			return Result<bool>.Fail(errors, "Erro de validação.");
		}

		var orderEvent = new PedidoCriadoEvent(request.Id, request.Cliente, request.Valor, request.DataPedido);

		_logger.LogInformation(
				"Publicando pedido {OrderId} na fila RabbitMQ",
				request.Id);

		await _publisher.PublishAsync(orderEvent);

		_logger.LogInformation(
			   "Pedido {OrderId} publicado na fila com sucesso. Cliente: {Cliente}, Valor: {Valor:C}",
			   request.Id,
			   request.Cliente,
			   request.Valor
		   );

		return Result<bool>.Ok(true);
	}
}