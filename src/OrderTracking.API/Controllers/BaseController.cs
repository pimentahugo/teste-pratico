using Microsoft.AspNetCore.Mvc;
using OrderTracking.API.Configs;
using OrderTracking.Shared.Results;
using System.Net;

namespace OrderTracking.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class BaseController : ControllerBase
{
	/// <summary>
	/// Retorna uma resposta padronizada com código HTTP definido manualmente.
	/// </summary>
	protected IActionResult CustomResponse<T>(Result<T> result, HttpStatusCode statusCode)
		=> ApiResponse<T>.CustomResponse(this, result, statusCode);
}