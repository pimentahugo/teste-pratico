using System.Net;

namespace OrderTracking.Domain.Exceptions;

/// <summary>
/// Exceção base da aplicação Order.	
/// Use essa exceção para criar exceções customizadas que devem ser tratadas pelo ExceptionFilter.
/// </summary>
public abstract class OrderException : SystemException
{
	protected OrderException(string message) : base(message) { }

	/// <summary>
	/// Retorna a lista de mensagens de erros.
	/// </summary>
	public abstract IList<string> GetErrorMessages();

	/// <summary>
	/// Retorna o status code HTTP apropriado para a exceção.
	/// </summary>
	public abstract HttpStatusCode GetStatusCode();
}