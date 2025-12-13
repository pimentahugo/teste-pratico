
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OrderTracking.Infrastructure.Services.Cache;
using OrderTracking.UnitTests.Builders;

namespace OrderTracking.IntegrationTests.Infrastructure;
public class OrderCacheServiceTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
	private readonly IntegrationTestWebAppFactory _factory;
	private readonly IOrderCacheService _cacheService;

	public OrderCacheServiceTests(IntegrationTestWebAppFactory factory)
	{
		_factory = factory;

		using var scope = factory.Services.CreateScope();
		_cacheService = scope.ServiceProvider.GetRequiredService<IOrderCacheService>();
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		await _factory.ResetDatabaseAsync();
	}

	[Fact]
	public async Task SetAndGet_ShouldCacheOrder()
	{
		// Arrange
		var order = new OrderFaker().Generate();

		// Act
		await _cacheService.SetOrderAsync(order);
		var cachedOrder = await _cacheService.GetOrderAsync(order.Id);

		// Assert
		cachedOrder.Should().NotBeNull();
		cachedOrder!.Cliente.Should().Be(order.Cliente);
	}

	[Fact]
	public async Task GetAsync_WithNonExistingOrder_ShouldReturnNull()
	{
		// Arrange
		var nonExistentId = Guid.NewGuid();

		// Act
		var result = await _cacheService.GetOrderAsync(nonExistentId);

		// Assert
		result.Should().BeNull();
	}
}