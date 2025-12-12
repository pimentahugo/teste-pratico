using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Data.Mongo;
using OrderTracking.Infrastructure.Data.Mongo.Models;
using OrderTracking.Infrastructure.Data.SqlServer;
using OrderTracking.Infrastructure.Messaging;
using OrderTracking.Infrastructure.Messaging.Settings;
using OrderTracking.Infrastructure.Repositories;
using OrderTracking.Infrastructure.Services.Cache;
using RabbitMQ.Client;

namespace OrderTracking.Infrastructure;
public static class DependencyInjection
{
	public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		AddEntityFramework(services, configuration);
		AddRepositories(services);
		AddRabbitMq(services, configuration);
		AddMongoDb(services, configuration);
	}

	private static void AddMongoDb(IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));

		services.AddSingleton(provider =>
		{
			return provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
		});

		services.AddSingleton<IMongoClient>(provider =>
		{
			var settings = provider.GetRequiredService<MongoDbSettings>();
			return new MongoClient(settings.ConnectionString);
		});

		services.AddSingleton<IMongoDatabase>(provider =>
		{
			var client = provider.GetRequiredService<IMongoClient>();
			var settings = provider.GetRequiredService<MongoDbSettings>();
			return client.GetDatabase(settings.DatabaseName);
		});

		services.AddSingleton<IMongoCollection<OrderCacheModel>>(provider =>
		{
			var database = provider.GetRequiredService<IMongoDatabase>();
			var settings = provider.GetRequiredService<MongoDbSettings>();

			var collection = database.GetCollection<OrderCacheModel>(settings.OrdersCollectionName);

			var indexKeys = Builders<OrderCacheModel>.IndexKeys.Ascending(x => x.ExpiresAt);
			var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
			var indexModel = new CreateIndexModel<OrderCacheModel>(indexKeys, indexOptions);

			collection.Indexes.CreateOneAsync(indexModel).GetAwaiter().GetResult();

			return collection;
		});

		services.AddSingleton<IOrderCacheService, OrderCacheService>();
	}

	private static void AddRabbitMq(IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

		services.AddSingleton(provider =>
		{
			return provider.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
		});

		services.AddSingleton<RabbitMqConnection>();

		services.AddSingleton<IConnection>(provider =>
		{
			var rabbitMqConnection = provider.GetRequiredService<RabbitMqConnection>();
			return rabbitMqConnection.GetConnection().GetAwaiter().GetResult();
		});

		services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
	}

	private static void AddRepositories(IServiceCollection services)
	{
		services.AddScoped<IOrderRepository, OrderRepository>();
	}

	private static void AddEntityFramework(IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<OrderTrackingContext>(options =>
		{
			var connectionString = configuration.GetConnectionString("SqlServer");
			options.UseSqlServer(connectionString, sqlOptions =>
			{
				sqlOptions.MigrationsAssembly(typeof(OrderTrackingContext).Assembly.FullName);
				sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
			});
		});
	}
}