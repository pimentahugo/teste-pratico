using OrderTracking.Infrastructure;
using OrderTracking.Shared.Observability;
using OrderTracking.Worker.Consumers;
using OrderTracking.Worker.Handlers;
using OrderTracking.Worker.Workers;
using Serilog;

try
{
	var builder = Host.CreateApplicationBuilder(args);

	builder.Services.AddSerilog();

	builder.Services.AddInfrastructure(builder.Configuration);

	builder.Services.AddScoped<IOrderEventHandler, OrderEventHandler>();

	builder.Services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();

	builder.Services.AddHostedService<RabbitMqConsumerWorker>();

	builder.Services.AddOpenTelemetryConfiguration("OrderTracking.Worker", includeAspNetCore: false);

	var host = builder.Build();

	await host.RunAsync();
}
catch (Exception ex)
{
	return 1;
}
finally
{
	
}

return 0;