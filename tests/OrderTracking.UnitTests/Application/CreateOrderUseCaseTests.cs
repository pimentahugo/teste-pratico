using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderTracking.Application.UseCases.Order.Create;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Infrastructure.Messaging.Events;
using OrderTracking.Shared.Messages;
using OrderTracking.UnitTests.Builders;

namespace OrderTracking.UnitTests.Application;
public class CreateOrderUseCaseTests
{
	private readonly Mock<IMessagePublisher> _mockPublisher;
	private readonly Mock<ILogger<CreateOrderUseCase>> _mockLogger;
	private readonly CreateOrderRequestValidator _validator;
	private readonly CreateOrderUseCase _useCase;
	private readonly CreateOrderRequestFaker _requestFaker;

	public CreateOrderUseCaseTests()
	{
		_mockPublisher = new Mock<IMessagePublisher>();
		_mockLogger = new Mock<ILogger<CreateOrderUseCase>>();
		_validator = new CreateOrderRequestValidator();

		_useCase = new CreateOrderUseCase(
			_validator,
			_mockPublisher.Object,
			_mockLogger.Object
		);

		_requestFaker = new CreateOrderRequestFaker();
	}

	[Fact]
	public async Task ExecuteAsync_WithValidRequest_ShouldReturnSuccess()
	{
		var request = _requestFaker.Generate();

		var result = await _useCase.ExecuteAsync(request);

		result.Should().NotBeNull();
		result.IsSuccess.Should().BeTrue();	
		result.Data.Should().BeTrue();

		_mockPublisher.Verify(
			x => x.PublishAsync(It.Is<PedidoCriadoEvent>(
				e => e.PedidoId == request.Id &&
					 e.Cliente == request.Cliente &&
					 e.Valor == request.Valor
			)),
			Times.Once
		);
	}

	[Fact]
	public async Task ExecuteAsync_WithInvalidCliente_ShouldReturnFailure()
	{
		var request = _requestFaker.WithInvalidCliente().Generate();

		var result = await _useCase.ExecuteAsync(request);

		result.Should().NotBeNull();
		result.IsSuccess.Should().BeFalse();
		result.Errors.Should().NotBeEmpty();
		result.Errors.Should().Contain(e => e.Contains(ValidationMessages.Pedido_ClienteObrigatorio));

		_mockPublisher.Verify(
			x => x.PublishAsync(It.IsAny<PedidoCriadoEvent>()),
			Times.Never
		);
	}

	[Fact]
	public async Task ExecuteAsync_WithInvalidValor_ShouldReturnFailure()
	{
		var request = _requestFaker.WithInvalidValor().Generate();

		var result = await _useCase.ExecuteAsync(request);

		result.IsSuccess.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains(ValidationMessages.Pedido_ValorInvalido));

		_mockPublisher.Verify(
			x => x.PublishAsync(It.IsAny<PedidoCriadoEvent>()),
			Times.Never
		);
	}

	[Fact]
	public async Task ExecuteAsync_WithLongCliente_ShouldReturnFailure()
	{
		var request = _requestFaker.WithLongCliente().Generate();

		var result = await _useCase.ExecuteAsync(request);

		result.IsSuccess.Should().BeFalse();
		result.Errors.Should().Contain(e => e.Contains(ValidationMessages.Pedido_ClienteMaxLength));
	}
}