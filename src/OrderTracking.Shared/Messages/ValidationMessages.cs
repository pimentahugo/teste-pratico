namespace OrderTracking.Shared.Messages;
public static class ValidationMessages
{
	public static string DomainRuleViolation => "Uma ou mais regras de domínio foram violadas.";


	public static string Pedido_IdObrigatorio => "O identificador do pedido é obrigatório.";
	public static string Pedido_ClienteObrigatorio => "O nome do cliente é obrigatório.";
	public static string Pedido_ClienteMaxLength => "O nome do cliente não pode exceder 100 caracteres.";
	public static string Pedido_ValorInvalido => "O valor do pedido deve ser maior que zero.";
	public static string Pedido_DataInvalida => "A data do pedido informada é invalida.";	
	public static string Order_NotFound => "O pedido informado não foi encontrado.";
}