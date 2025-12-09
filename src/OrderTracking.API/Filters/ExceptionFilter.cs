using Microsoft.AspNetCore.Mvc.Filters;
using OrderTracking.Domain.Exceptions;

namespace OrderTracking.API.Filters;
public class ExceptionFilter : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		if(context.Exception is OrderException orderException)
		{
			HandleProjectException(context, orderException);
		} else
		{
			ThrowUnkownException(context);
		}
	}

	private void HandleProjectException(ExceptionContext context, OrderException orderException)
	{

		var statusCode = (int)orderException.GetStatusCode();
		var errors = orderException.GetErrorMessages();

	}

	private void ThrowUnkownException(ExceptionContext context)
	{

	}
}