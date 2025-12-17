using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace OrderTracking.Shared.Observability;
public static class SerilogConfiguration
{
	public static ILogger CreateLogger(string applicationName, string version = "1.0.0")
	{
		return new LoggerConfiguration()
			.MinimumLevel.Information()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
			.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
			.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
			.MinimumLevel.Override("System", LogEventLevel.Warning)
			.MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)

			// Enrichers
			.Enrich.FromLogContext()
			.Enrich.WithMachineName()
			.Enrich.WithEnvironmentName()
			.Enrich.WithThreadId()
			.Enrich.WithExceptionDetails()
			.Enrich.WithProperty("Application", applicationName)
			.Enrich.WithProperty("Version", version)

			// Sinks
			.WriteTo.Console(new CompactJsonFormatter())
			.WriteTo.File(
				new CompactJsonFormatter(),
				path: $"logs/{applicationName.ToLower()}-.log",
				rollingInterval: RollingInterval.Day,
				retainedFileCountLimit: 30,
				fileSizeLimitBytes: 10_000_000,
				rollOnFileSizeLimit: true
			)
			.CreateLogger();
	}
}