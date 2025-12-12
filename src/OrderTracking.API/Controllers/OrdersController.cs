using Microsoft.AspNetCore.Mvc;
using OrderTracking.API.Configs;
using OrderTracking.Application.Responses;
using OrderTracking.Application.UseCases.Order.Create;
using OrderTracking.Application.UseCases.Order.GetById;
using System.Net;

namespace OrderTracking.API.Controllers;
public class OrdersController : BaseController
{
	[HttpPost]
	[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Create(
		[FromServices] ICreateOrderUseCase useCase,
		[FromBody] CreateOrderRequest request)
	{
		var result = await useCase.ExecuteAsync(request);

		if(!result.IsSucess)
		{
			return CustomResponse(result, HttpStatusCode.BadRequest);
		}

		return CustomResponse(result, HttpStatusCode.Created);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(
		[FromServices] IOrderGetByIdUseCase useCase,
		[FromRoute] Guid id)
	{
		var result = await useCase.ExecuteAsync(id);

		if(!result.IsSucess)
		{
			return CustomResponse(result, HttpStatusCode.NotFound);
		}

		return CustomResponse(result, HttpStatusCode.OK);
	}
}