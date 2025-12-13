using FluentAssertions;
using OrderTracking.API.Configs;
using OrderTracking.Application.Responses;
using OrderTracking.IntegrationTests.Infrastructure;
using OrderTracking.UnitTests.Builders;
using System.Net;
using System.Net.Http.Json;

namespace OrderTracking.IntegrationTests.API;
public class OrdersControllerTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
	private readonly IntegrationTestWebAppFactory _factory;
	private readonly HttpClient _client;
	private readonly string _endpoint = "/api/orders";

	public OrdersControllerTest(IntegrationTestWebAppFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task GetOrderById_WithExistingOrder_ShouldReturn200Ok()
	{
		// Arrange 
		var request = new CreateOrderRequestFaker().Generate();	

		await _client.PostAsJsonAsync(_endpoint, request);

		// Aguardar processamento pelo Worker
		await Task.Delay(3000);

		// Act
		var response = await _client.GetAsync($"{_endpoint}/{request.Id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderResponse>>();
		result.Should().NotBeNull();
		result!.Data.Should().NotBeNull();
		result.Data!.Id.Should().Be(request.Id);
		result.Data.Cliente.Should().Be(request.Cliente);
		result.Data.Valor.Should().Be(request.Valor);
	}

	[Fact]
	public async Task CreateOrder_WithValidData_ShouldReturn202Accepted()
	{
		// Arrange
		var request = new CreateOrderRequestFaker().Generate();

		// Act
		var response = await _client.PostAsJsonAsync(_endpoint, request);

		//Assert
		var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
		result.Should().NotBeNull();
		result.Data.Should().BeTrue();	
	}

	[Fact]
	public async Task GetOrderById_WithNonExistingOrder_ShouldReturn404NotFound()
	{
		// Arrange
		var nonExistentId = Guid.NewGuid();

		// Act
		var response = await _client.GetAsync($"{_endpoint}/{nonExistentId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task CreateOrder_WithInvalidClienteVariations_ShouldReturn400()
	{
		// Arrange
		var request = new CreateOrderRequestFaker().WithEmptyCliente().Generate();

		// Act
		var response = await _client.PostAsJsonAsync(_endpoint, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		await _factory.ResetDatabaseAsync();
	}
}