using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderTracking.Domain.Repositories;
using OrderTracking.Infrastructure.Data.Mongo;
using OrderTracking.Infrastructure.Data.Mongo.Models;
using OrderTracking.Infrastructure.Data.SqlServer;
using OrderTracking.Infrastructure.Repositories;
using OrderTracking.Infrastructure.Services.Cache;

namespace OrderTracking.Infrastructure;
public static class DependencyInjection
{
	public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		AddEntityFramework(services, configuration);
		AddRepositories(services);
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