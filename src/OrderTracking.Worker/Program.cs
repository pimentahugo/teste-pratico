using OrderTracking.Infrastructure;
using OrderTracking.Shared.Observability;
using OrderTracking.Worker.Consumers;
using OrderTracking.Worker.Handlers;
using OrderTracking.Worker.Workers;
using Serilog;

Log.Logger = SerilogConfiguration.CreateLogger("OrderTracking.Worker", "1.0.0");
try
{
	Log.Information("Iniciando OrderTracking.Worker");

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
	Log.Fatal(ex, " Worker finalizado inesperadamente!");
	return 1;
}
finally
{
	Log.CloseAndFlush();
}

return 0;