using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrderTracking.API.Configs;
using OrderTracking.Domain.Exceptions;
using OrderTracking.Shared.Messages;
using System.Diagnostics;
using System.Net;

namespace OrderTracking.API.Filters;
public class ExceptionFilter : IExceptionFilter
{
	private readonly ILogger<ExceptionFilter> _logger;
	private readonly IWebHostEnvironment _environment;

	public ExceptionFilter(ILogger<ExceptionFilter> logger, IWebHostEnvironment environment)
	{
		_logger = logger;
		_environment = environment;
	}

	public void OnException(ExceptionContext context)
	{
		var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

		if (context.Exception is OrderException orderException)
		{
			HandleProjectException(context, orderException, traceId);
		}
		else
		{
			ThrowUnkownException(context, traceId);
		}
	}

	private void HandleProjectException(ExceptionContext context, OrderException orderException, string traceId)
	{
		_logger.LogWarning(
			orderException,
			"Exceção de domínio registrada. TraceId: {TraceId}, StatusCode: {StatusCode}, Message: {Message}",
			traceId,
			orderException.GetStatusCode(),
			orderException.GetErrorMessages()
		);

		var statusCode = (int)orderException.GetStatusCode();
		var errors = orderException.GetErrorMessages();

		var response = ApiResponse<object>.Fail(errors, orderException.Message);

		context.Result = new ObjectResult(response)
		{
			StatusCode = (int)orderException.GetStatusCode()
		};

		context.ExceptionHandled = true;

	}

	private void ThrowUnkownException(ExceptionContext context, string traceId)
	{
		_logger.LogError(
			context.Exception,
			"Exceção não tratada. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
			traceId,
			context.HttpContext.Request.Path,
			context.HttpContext.Request.Method
		);

		var message = _environment.IsDevelopment()
			? context.Exception.Message
			: "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.";

		var errors = _environment.IsDevelopment()
			? new[] { context.Exception.Message }
			: null;

		var response = ApiResponse<object>.Fail(errors, ValidationMessages.ErroInternoServidor);

		context.Result = new ObjectResult(response)
		{
			StatusCode = (int)HttpStatusCode.InternalServerError
		};

		context.ExceptionHandled = true;
	}
}