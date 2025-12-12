using OrderTracking.Infrastructure;
using OrderTracking.Worker.Consumers;
using OrderTracking.Worker.Handlers;
using OrderTracking.Worker.Workers;

try
{
	var builder = Host.CreateApplicationBuilder(args);

	builder.Services.AddInfrastructure(builder.Configuration);

	builder.Services.AddScoped<IOrderEventHandler, OrderEventHandler>();

	builder.Services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();

	builder.Services.AddHostedService<RabbitMqConsumerWorker>();

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