
using OrderTracking.Worker.Consumers;

namespace OrderTracking.Worker.Workers;
public class RabbitMqConsumerWorker : BackgroundService
{
	private readonly IMessageConsumer _messageConsumer;
	private readonly ILogger<RabbitMqConsumerWorker> _logger;

	public RabbitMqConsumerWorker(
		IMessageConsumer messageConsumer,
		ILogger<RabbitMqConsumerWorker> logger)
	{
		_messageConsumer = messageConsumer;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			_logger.LogInformation(
				"RabbitMQ Consumer Worker iniciando. Aguardando mensagens...");

			await _messageConsumer.StartConsumingAsync(stoppingToken);

			_logger.LogInformation(
				"RabbitMQ Consumer Worker iniciado com sucesso");

			await Task.Delay(Timeout.Infinite, stoppingToken);
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation(
				"RabbitMQ Consumer Worker sendo encerrado (shutdown solicitado)");
		}
		catch (Exception ex)
		{
			_logger.LogCritical(
				ex,
				"Erro fatal no RabbitMQ Consumer Worker. Worker será encerrado");

			throw;
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Parando RabbitMQ Consumer Worker...");

		await _messageConsumer.StopConsumingAsync();
		await base.StopAsync(cancellationToken);

		_logger.LogInformation(
			"RabbitMQ Consumer Worker parado com sucesso");
	}

	public override void Dispose()
	{
		_logger.LogDebug("Disposing RabbitMQ Consumer Worker");
		base.Dispose();
	}
}