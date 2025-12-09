using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Data.SqlServer;
using OrderTracking.Infrastructure.Messaging;
using OrderTracking.Infrastructure.Messaging.Settings;
using OrderTracking.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace OrderTracking.Infrastructure;
public static class DependencyInjection
{
	public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		AddEntityFramework(services, configuration);
		AddRepositories(services);
		AddRabbitMq(services, configuration);
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