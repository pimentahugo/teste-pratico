using Bogus;
using OrderTracking.Domain.Entities;

namespace OrderTracking.UnitTests.Builders;

public class OrderFaker : Faker<Order>
{
	public OrderFaker()
	{
		CustomInstantiator(f => Order.Criar(
			id: f.Random.Guid(),
			cliente: f.Person.FullName,
			valor: f.Finance.Amount(10, 10000),
			dataPedido: f.Date.Recent(30)
		));
	}

	public OrderFaker WithId(Guid id)
	{
		CustomInstantiator(f => Order.Criar(
			id: id,
			cliente: f.Person.FullName,
			valor: f.Finance.Amount(10, 10000),
			dataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public OrderFaker WithCliente(string cliente)
	{
		CustomInstantiator(f => Order.Criar(
			id: f.Random.Guid(),
			cliente: cliente,
			valor: f.Finance.Amount(10, 10000),
			dataPedido: f.Date.Recent(30)
		));
		return this;
	}

	public OrderFaker WithValor(decimal valor)
	{
		CustomInstantiator(f => Order.Criar(
			id: f.Random.Guid(),
			cliente: f.Person.FullName,
			valor: valor,
			dataPedido: f.Date.Recent(30)
		));
		return this;
	}
}