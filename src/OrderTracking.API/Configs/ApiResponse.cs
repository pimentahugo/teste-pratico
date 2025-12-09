using Microsoft.AspNetCore.Mvc;
using OrderTracking.Shared.Results;
using System.Net;

namespace OrderTracking.API.Configs;
public class ApiResponse<T>
{
	public string? Message { get; init; }
	public T? Data { get; init; }
	public IEnumerable<string>? Errors { get; init; }

	public static IActionResult CustomResponse(
		ControllerBase controller,
		Result<T> result,
		HttpStatusCode statusCode)
	{
		var response = new ApiResponse<T>
		{
			Message = result.Message,
			Data = result.Data,
			Errors = result.Errors
		};

		return controller.StatusCode((int)statusCode, response);
	}
}