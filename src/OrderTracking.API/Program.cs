using Microsoft.EntityFrameworkCore;
using OrderTracking.Application;
using OrderTracking.Infrastructure;
using OrderTracking.Infrastructure.Data.SqlServer;
using OrderTracking.Shared.Observability;
using Serilog;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = SerilogConfiguration.CreateLogger("OrderTracking.API", "1.0.0");

try
{
	Log.Information("Iniciando OrderTracking.API");

	builder.Host.UseSerilog();

	builder.Services.AddControllers();
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();

	builder.Services.AddApplication(builder.Configuration);
	builder.Services.AddInfrastructure(builder.Configuration);

	builder.Services.AddOpenTelemetryConfiguration("OrderTracking.API", includeAspNetCore: true);

	builder.Services.AddCors(options =>
	{
		options.AddPolicy("AllowAll", policy =>
		{
			policy.AllowAnyOrigin()
				  .AllowAnyMethod()
				  .AllowAnyHeader();
		});
	});

	var app = builder.Build();

	// Migrations
	using (var scope = app.Services.CreateScope())
	{
		var services = scope.ServiceProvider;
		try
		{
			var context = services.GetRequiredService<OrderTrackingContext>();
			var logger = services.GetRequiredService<ILogger<Program>>();

			logger.LogInformation("Aplicando migrations do banco de dados...");
			await context.Database.MigrateAsync();
			logger.LogInformation("Migrations aplicadas com sucesso!");
		}
		catch (Exception ex)
		{
			var logger = services.GetRequiredService<ILogger<Program>>();
			logger.LogError(ex, "Erro ao aplicar migrations");
			throw;
		}
	}

	app.UseSerilogRequestLogging(options =>
	{
		options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
		options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
		{
			diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
			diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
			diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
		};
	});

	app.UseCors("AllowAll");

	app.UseSwagger();
	app.UseSwaggerUI();

	app.UseRouting();
	app.UseHttpMetrics();

	app.UseAuthorization();
	app.MapControllers();
	app.MapMetrics();


	app.UseOpenTelemetryPrometheusScrapingEndpoint();

	app.MapGet("/health", () => Results.Ok(new
	{
		status = "healthy",
		timestamp = DateTime.UtcNow,
		application = "OrderTracking.API",
		version = "1.0.0"
	}));

	Log.Information("OrderTracking.API iniciada com sucesso!");

	await app.RunAsync();
} catch(Exception ex)
{
	Log.Fatal(ex, "Aplicação finalizada inesperadamente!");
}
finally
{
	Log.CloseAndFlush();
}

public partial class Program { }