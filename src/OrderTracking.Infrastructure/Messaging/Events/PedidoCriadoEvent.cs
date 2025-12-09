namespace OrderTracking.Infrastructure.Messaging.Events;
public class PedidoCriadoEvent
{
	public Guid PedidoId { get; set; }
	public string Cliente { get; set; } = string.Empty;
	public decimal Valor { get; set; }
	public DateTime DataPedido { get; set; }


	public PedidoCriadoEvent(Guid pedidoId, string cliente, decimal valor, DateTime dataPedido)
	{
		PedidoId = pedidoId;
		Cliente = cliente;
		Valor = valor;
		DataPedido = dataPedido;
	}
}