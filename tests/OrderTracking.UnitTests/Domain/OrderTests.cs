using FluentAssertions;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Exceptions;
using OrderTracking.Shared.Messages;
using OrderTracking.UnitTests.Builders;

namespace OrderTracking.UnitTests.Domain;

public class OrderTests
{
	private readonly OrderFaker _orderFaker;

	public OrderTests()
	{
		_orderFaker = new OrderFaker();
	}

	[Fact]
	public void Criar_WithValidData_ShouldCreateOrder()
	{
		var orderData = _orderFaker.Generate();

		var order = Order.Criar(
			orderData.Id,
			orderData.Cliente,
			orderData.Valor,
			orderData.DataPedido
		);

		order.Should().NotBeNull();
		order.Id.Should().Be(orderData.Id);
		order.Cliente.Should().Be(orderData.Cliente);
		order.Valor.Should().Be(orderData.Valor);
		order.DataPedido.Should().Be(orderData.DataPedido);
	}

	[Fact]
	public void Criar_WithEmptyGuid_ShouldThrowDomainException()
	{
		var orderData = _orderFaker.Generate();

		Action act = () => Order.Criar(
			Guid.Empty,
			orderData.Cliente,
			orderData.Valor,
			orderData.DataPedido
		);

		act.Should().Throw<DomainException>()
			.WithMessage(ValidationMessages.Pedido_IdObrigatorio);
	}

	[Theory]
	[InlineData("")]
	[InlineData(null)]
	[InlineData(" ")]
	[InlineData("   ")]
	public void Criar_WithInvalidCliente_ShouldThrowDomainException(string clienteInvalid)
	{
		var orderData = _orderFaker.Generate();

		Action act = () => Order.Criar(
			orderData.Id,
			clienteInvalid,
			orderData.Valor,
			orderData.DataPedido
		);

		act.Should().Throw<DomainException>()
			.WithMessage(ValidationMessages.Pedido_ClienteObrigatorio);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(-100)]
	[InlineData(-1000.50)]
	public void Criar_WithInvalidValor_ShouldThrowDomainException(decimal valorInvalid)
	{
		var orderData = _orderFaker.Generate();

		Action act = () => Order.Criar(
			orderData.Id,
			orderData.Cliente,
			valorInvalid,
			orderData.DataPedido
		);

		act.Should().Throw<DomainException>()
			.WithMessage(ValidationMessages.Pedido_ValorInvalido);
	}

	[Fact]
	public void Criar_WithMinDate_ShouldThrowDomainException()
	{
		var orderData = _orderFaker.Generate();

		Action act = () => Order.Criar(
			orderData.Id,
			orderData.Cliente,
			orderData.Valor,
			DateTime.MinValue
		);

		act.Should().Throw<DomainException>()
			.WithMessage(ValidationMessages.Pedido_DataInvalida);
	}
}