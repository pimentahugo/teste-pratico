namespace OrderTracking.Application.Responses;
public record OrderResponse(
	Guid Id,
	string Cliente,
	decimal Valor,
	DateTime DataPedido,
	DateTime DataCriacao
);