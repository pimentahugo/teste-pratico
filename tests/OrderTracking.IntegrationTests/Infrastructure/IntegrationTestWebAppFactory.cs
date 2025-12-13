using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Data.Mongo;
using OrderTracking.Infrastructure.Data.Mongo.Models;
using OrderTracking.Infrastructure.Data.SqlServer;
using OrderTracking.Infrastructure.Messaging.Settings;
using OrderTracking.Infrastructure.Repositories;
using OrderTracking.Infrastructure.Services.Cache;
using OrderTracking.Worker.Consumers;
using OrderTracking.Worker.Handlers;
using OrderTracking.Worker.Workers;
using RabbitMQ.Client;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace OrderTracking.IntegrationTests.Infrastructure;
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	private readonly MsSqlContainer _sqlContainer;
	private readonly MongoDbContainer _mongoContainer;
	private readonly RabbitMqContainer _rabbitMqContainer;
	private IHost? _workerHost;

	// Propriedades para compartilhar configurações
	private string? _sqlConnectionString;
	private string? _mongoConnectionString;
	private RabbitMqSettings? _rabbitMqSettings;
	private MongoDbSettings? _mongoDbSettings;

	public IntegrationTestWebAppFactory()
	{
		_sqlContainer = new MsSqlBuilder()
			 .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
			 .WithPassword("Teste123!")
			 .Build();

		_mongoContainer = new MongoDbBuilder()
			.WithImage("mongo:7.0")
			.Build();

		_rabbitMqContainer = new RabbitMqBuilder()
			.WithImage("rabbitmq:3-management")
			.WithUsername("guest")
			.WithPassword("guest")
			.Build();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			ConfigureSqlServer(services);
			ConfigureMongoDb(services);
			ConfigureRabbitMq(services);
		});

		builder.UseEnvironment("Test");
	}

	public async Task InitializeAsync()
	{
		// Iniciar containers
		await Task.WhenAll(
			_sqlContainer.StartAsync(),
			_mongoContainer.StartAsync(),
			_rabbitMqContainer.StartAsync()
		);

		// Aguardar RabbitMQ estar pronto
		await Task.Delay(5000);

		// Preparar configurações após containers estarem prontos
		_sqlConnectionString = _sqlContainer.GetConnectionString();
		_mongoConnectionString = _mongoContainer.GetConnectionString();

		_rabbitMqSettings = new RabbitMqSettings
		{
			HostName = _rabbitMqContainer.Hostname,
			Port = _rabbitMqContainer.GetMappedPublicPort(5672),
			UserName = "guest",
			Password = "guest",
			QueueName = "orders-test"
		};

		_mongoDbSettings = new MongoDbSettings
		{
			ConnectionString = _mongoConnectionString!,
			DatabaseName = "OrderTrackingCacheTest",
			OrdersCollectionName = "orders",
			CacheExpirationMinutes = 30
		};

		// Aplicar migrations
		using var scope = Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<OrderTrackingContext>();
		await dbContext.Database.MigrateAsync();

		// Iniciar Worker
		await StartWorkerAsync();
	}

	private async Task StartWorkerAsync()
	{
		_workerHost = Host.CreateDefaultBuilder()
					   .ConfigureLogging(logging =>
					   {
						   logging.ClearProviders();
						   logging.AddConsole();
						   logging.SetMinimumLevel(LogLevel.Debug);
					   })
					   .ConfigureServices((context, services) =>
					   {
						   // Remover configurações existentes
						   services.RemoveAll<DbContextOptions<OrderTrackingContext>>();
						   services.RemoveAll<OrderTrackingContext>();
						   services.RemoveAll<IMongoClient>();
						   services.RemoveAll<IMongoDatabase>();
						   services.RemoveAll<MongoDbSettings>();
						   services.RemoveAll<IConnection>();
						   services.RemoveAll<RabbitMqSettings>();

						   // Configurar com as mesmas configurações da API
						   ConfigureSqlServerForWorker(services);
						   ConfigureMongoDbForWorker(services);
						   ConfigureRabbitMqForWorker(services);

						   // Registrar os serviços necessários para o Worker
						   services.AddScoped<IOrderRepository, OrderRepository>();
						   services.AddSingleton<IOrderCacheService, OrderCacheService>();
						   services.AddScoped<IOrderEventHandler, OrderEventHandler>();
						   services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
						   services.AddHostedService<RabbitMqConsumerWorker>();
					   })
					   .Build();

		await _workerHost.StartAsync();

		// Aguardar o Worker inicializar completamente
		await Task.Delay(2000);
	}

	async Task IAsyncLifetime.DisposeAsync()
	{
		// Pare o Worker antes de descartar os containers
		if (_workerHost != null)
		{
			await _workerHost.StopAsync();
			_workerHost.Dispose();
		}

		await Task.WhenAll(
			_sqlContainer.DisposeAsync().AsTask(),
			_mongoContainer.DisposeAsync().AsTask(),
			_rabbitMqContainer.DisposeAsync().AsTask()
		);
	}

	public async Task ResetDatabaseAsync()
	{
		using var scope = Services.CreateScope();

		var dbContext = scope.ServiceProvider.GetRequiredService<OrderTrackingContext>();
		await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Orders");

		var mongoDb = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
		await mongoDb.DropCollectionAsync("orders");
	}

	// Configurações para a API (WebApplicationFactory)
	private void ConfigureRabbitMq(IServiceCollection services)
	{
		services.RemoveAll<IConnection>();
		services.RemoveAll<RabbitMqSettings>();

		services.AddSingleton(_rabbitMqSettings!);
		services.AddSingleton<IConnection>(sp =>
		{
			var factory = new ConnectionFactory
			{
				HostName = _rabbitMqSettings!.HostName,
				Port = _rabbitMqSettings.Port,
				UserName = _rabbitMqSettings.UserName,
				Password = _rabbitMqSettings.Password
			};
			return factory.CreateConnectionAsync().GetAwaiter().GetResult();
		});
	}

	private void ConfigureSqlServer(IServiceCollection services)
	{
		services.RemoveAll<DbContextOptions<OrderTrackingContext>>();
		services.RemoveAll<OrderTrackingContext>();

		services.AddDbContext<OrderTrackingContext>(options =>
		{
			options.UseSqlServer(_sqlConnectionString);
		});
	}

	private void ConfigureMongoDb(IServiceCollection services)
	{
		services.RemoveAll<IMongoClient>();
		services.RemoveAll<IMongoDatabase>();
		services.RemoveAll<MongoDbSettings>();

		services.AddSingleton(_mongoDbSettings!);
		services.AddSingleton<IMongoClient>(_ => new MongoClient(_mongoDbSettings!.ConnectionString));
		services.AddSingleton(sp =>
		{
			var client = sp.GetRequiredService<IMongoClient>();
			return client.GetDatabase(_mongoDbSettings!.DatabaseName);
		});

		// Registre a coleção para o cache
		services.AddSingleton<IMongoCollection<OrderCacheModel>>(sp =>
		{
			var database = sp.GetRequiredService<IMongoDatabase>();
			return database.GetCollection<OrderCacheModel>(_mongoDbSettings!.OrdersCollectionName);
		});
	}

	// Configurações para o Worker (Host)
	private void ConfigureRabbitMqForWorker(IServiceCollection services)
	{
		services.AddSingleton(_rabbitMqSettings!);
		services.AddSingleton<IConnection>(sp =>
		{
			var factory = new ConnectionFactory
			{
				HostName = _rabbitMqSettings!.HostName,
				Port = _rabbitMqSettings.Port,
				UserName = _rabbitMqSettings.UserName,
				Password = _rabbitMqSettings.Password
			};
			return factory.CreateConnectionAsync().GetAwaiter().GetResult();
		});
	}

	private void ConfigureSqlServerForWorker(IServiceCollection services)
	{
		services.AddDbContext<OrderTrackingContext>(options =>
		{
			options.UseSqlServer(_sqlConnectionString);
		});
	}

	private void ConfigureMongoDbForWorker(IServiceCollection services)
	{
		services.AddSingleton(_mongoDbSettings!);
		services.AddSingleton<IMongoClient>(_ => new MongoClient(_mongoDbSettings!.ConnectionString));
		services.AddSingleton(sp =>
		{
			var client = sp.GetRequiredService<IMongoClient>();
			return client.GetDatabase(_mongoDbSettings!.DatabaseName);
		});

		services.AddSingleton<IMongoCollection<OrderCacheModel>>(sp =>
		{
			var database = sp.GetRequiredService<IMongoDatabase>();
			return database.GetCollection<OrderCacheModel>(_mongoDbSettings!.OrdersCollectionName);
		});
	}
}