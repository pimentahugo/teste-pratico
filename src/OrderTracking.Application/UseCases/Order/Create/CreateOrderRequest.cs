using FluentValidation;
using OrderTracking.Shared.Messages;

namespace OrderTracking.Application.UseCases.Order.Create;
public record CreateOrderRequest(Guid Id, string Cliente, DateTime DataPedido, decimal Valor);

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
	public CreateOrderRequestValidator()
	{
		RuleFor(x => x.Id).NotEmpty().WithMessage(ValidationMessages.Pedido_IdObrigatorio);
		RuleFor(x => x.Cliente).NotEmpty().WithMessage(ValidationMessages.Pedido_ClienteObrigatorio)
			.MaximumLength(100).WithMessage(ValidationMessages.Pedido_ClienteMaxLength);
		RuleFor(x => x.DataPedido).GreaterThan(DateTime.MinValue).WithMessage(ValidationMessages.Pedido_DataInvalida);
		RuleFor(x => x.Valor).GreaterThan(0).WithMessage(ValidationMessages.Pedido_ValorInvalido);
	}
}