using OrderTracking.Shared.Messages;
using System.Net;

namespace OrderTracking.Domain.Exceptions;


/// <summary>
/// Exception lançada quando uma regra de domínio é violada.
/// Use para validações e regras específicas do Domínio.
/// </summary>

public sealed class DomainException : OrderException
{
	private readonly List<string> _errors;

	public DomainException(string message) : base(message)
	{
		_errors = new List<string> { message };
	}

	public DomainException(IEnumerable<string> errors) : base(ValidationMessages.DomainRuleViolation)
	{
		_errors = new List<string>(errors);
	}

	public override IList<string> GetErrorMessages() => _errors;

	public override HttpStatusCode GetStatusCode() => HttpStatusCode.BadRequest;
}