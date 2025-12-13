using Bogus;
using OrderTracking.Application.UseCases.Order.Create;

namespace OrderTracking.UnitTests.Builders;

public class CreateOrderRequestFaker : Faker<CreateOrderRequest>
{
	public CreateOrderRequestFaker()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: f.Person.FullName,
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: f.Date.Recent(30)
		));
	}

	public CreateOrderRequestFaker WithEmptyId()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: Guid.Empty,
			Cliente: f.Person.FullName,
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public CreateOrderRequestFaker WithEmptyCliente()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: string.Empty,
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public CreateOrderRequestFaker WithNullCliente()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: null!,
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public CreateOrderRequestFaker WithWhitespaceCliente()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: "   ",
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public CreateOrderRequestFaker WithInvalidCliente()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: string.Empty, // Inválido
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: f.Date.Recent(30)
		));
		return this;
	}


	public CreateOrderRequestFaker WithLongCliente()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: f.Random.String2(201),
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public CreateOrderRequestFaker WithInvalidValor()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: f.Person.FullName,
			Valor: -1, // Inválido
			DataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public CreateOrderRequestFaker WithMinDate()
	{
		CustomInstantiator(f => new CreateOrderRequest(
			Id: f.Random.Guid(),
			Cliente: f.Person.FullName,
			Valor: f.Finance.Amount(10, 10000),
			DataPedido: DateTime.MinValue
		));
		return this;
	}
}