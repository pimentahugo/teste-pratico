using OrderTracking.Domain.Exceptions;
using OrderTracking.Shared.Messages;

namespace OrderTracking.Domain.Entities;
public class Order
{
	public Guid Id { get; private set; }
	public string Cliente { get; private set; } = string.Empty;
	public decimal Valor { get; private set ; } 
	public DateTime DataPedido { get; private set; } //data cliente
	public DateTime DataCriacao { get; private set; } //data sistema

	//EF Core requires an empty constructor
	protected Order() { }

	private Order(Guid id, string cliente, decimal valor, DateTime dataPedido)
	{
		Id = id;
		Cliente = cliente;
		Valor = valor;
		DataPedido = dataPedido;
		DataCriacao = DateTime.Now;

		Validar();
	}

	public static Order Criar(Guid id, string cliente, decimal valor, DateTime dataPedido)
	{
		return new Order(
			id: id,
			cliente: cliente,
			valor: valor,
			dataPedido: dataPedido);
	}

	public static Order FromCache(Guid id, string cliente, decimal valor, DateTime dataPedido, DateTime dataCriacao)
	{
		return new Order(id, cliente, valor, dataPedido)
		{
			DataCriacao = dataCriacao
		};
	}

	private void Validar()
	{
		if (Id == Guid.Empty)
			throw new DomainException(ValidationMessages.Pedido_IdObrigatorio);
		if(string.IsNullOrWhiteSpace(Cliente))
			throw new DomainException(ValidationMessages.Pedido_ClienteObrigatorio);
		if (Valor <= 0)
			throw new DomainException(ValidationMessages.Pedido_ValorInvalido);
		if (DataPedido == DateTime.MinValue)
			throw new DomainException(ValidationMessages.Pedido_DataInvalida);
	}
}