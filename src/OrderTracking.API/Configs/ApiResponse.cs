using Microsoft.AspNetCore.Mvc;
using OrderTracking.Shared.Results;
using System.Net;

namespace OrderTracking.API.Configs;

public class ApiResponse<T>
{
	public string? Message { get; init; }
	public T? Data { get; init; }
	public IEnumerable<string>? Errors { get; init; }

	private ApiResponse(string? message, T? data, IEnumerable<string>? errors)
	{
		Message = message;
		Data = data;
		Errors = errors;
	}

	public static ApiResponse<T> Success(T data, string? message = null)
		=> new(message, data, null);

	public static ApiResponse<T> Fail(IEnumerable<string>? errors, string? message = null)
		=> new(message, default, errors);

	public static IActionResult CustomResponse(
		ControllerBase controller,
		Result<T> result,
		HttpStatusCode statusCode)
	{
		var response = new ApiResponse<T>(
			message: result.Message,
			data: result.Data,
			errors: result.Errors
		);

		return controller.StatusCode((int)statusCode, response);
	}
}
