using FluentAssertions;
using Moq;
using OrderTracking.Application.UseCases.Order.GetById;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Repositories;
using OrderTracking.Shared.Messages;
using OrderTracking.UnitTests.Builders;

namespace OrderTracking.UnitTests.Application.UseCases;

public class GetOrderByIdUseCaseTests
{
	private readonly Mock<IOrderRepository> _mockRepository;
	private readonly OrderGetByIdUseCase _useCase;
	private readonly OrderFaker _orderFaker;

	public GetOrderByIdUseCaseTests()
	{
		_mockRepository = new Mock<IOrderRepository>();
		_useCase = new OrderGetByIdUseCase(_mockRepository.Object);
		_orderFaker = new OrderFaker();
	}

	[Fact]
	public async Task ExecuteAsync_WithExistingOrder_ShouldReturnOrder()
	{
		// Arrange
		var order = _orderFaker.Generate();

		_mockRepository
			.Setup(x => x.GetByIdAsync(order.Id))
			.ReturnsAsync(order);

		// Act
		var result = await _useCase.ExecuteAsync(order.Id);

		// Assert
		result.Should().NotBeNull();
		result.IsSuccess.Should().BeTrue();
		result.Data.Should().NotBeNull();
		result.Data!.Id.Should().Be(order.Id);
		result.Data.Cliente.Should().Be(order.Cliente);
		result.Data.Valor.Should().Be(order.Valor);
	}

	[Fact]
	public async Task ExecuteAsync_WithNonExistingOrder_ShouldReturnNotFound()
	{
		// Arrange
		var orderId = Guid.NewGuid();

		_mockRepository
			.Setup(x => x.GetByIdAsync(orderId))
			.ReturnsAsync((Order?)null);

		// Act
		var result = await _useCase.ExecuteAsync(orderId);

		// Assert
		result.Should().NotBeNull();
		result.IsSuccess.Should().BeFalse();
		result.Data.Should().BeNull();
		result.Message.Should().Contain(ValidationMessages.Pedido_NotFound);
	}
}