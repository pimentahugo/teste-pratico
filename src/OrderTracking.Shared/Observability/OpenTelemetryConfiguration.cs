using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OrderTracking.Shared.Observability;

public static class OpenTelemetryConfiguration
{
	public static IServiceCollection AddOpenTelemetryConfiguration(
		this IServiceCollection services,
		string serviceName,
		bool includeAspNetCore = false)
	{
		// Tracing
		services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(serviceName))
			.WithTracing(tracing =>
			{
				tracing
					.AddSource(serviceName);

				// ASP.NET Core instrumentation (só para API)
				if (includeAspNetCore)
				{
					tracing.AddAspNetCoreInstrumentation(options =>
					{
						options.RecordException = true;
						options.Filter = httpContext =>
						{
							// Não trace health checks
							return !httpContext.Request.Path.StartsWithSegments("/health");
						};
					});
				}

				tracing
					.AddHttpClientInstrumentation()
					.AddConsoleExporter();
			});

		// Metrics
		services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics
					.AddMeter(serviceName);

				// ASP.NET Core metrics (só para API)
				if (includeAspNetCore)
				{
					metrics.AddAspNetCoreInstrumentation();
				}

				metrics
					.AddHttpClientInstrumentation()
					.AddRuntimeInstrumentation()
					.AddProcessInstrumentation();

				// Prometheus exporter (só para API)
				if (includeAspNetCore)
				{
					metrics.AddPrometheusExporter();
				}
			});

		return services;
	}
}