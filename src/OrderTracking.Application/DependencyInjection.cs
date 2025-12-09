using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderTracking.Application.UseCases.Order.Create;

namespace OrderTracking.Application;
public static class DependencyInjection
{
	public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
	{
		AddValidators(services);
		AddUseCases(services);
	}

	private static void AddValidators(IServiceCollection services)
	{
		services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
	}

	private static void AddUseCases(IServiceCollection services)
	{
		services.AddScoped<ICreateOrderUseCase, CreateOrderUseCase>();
	}
}