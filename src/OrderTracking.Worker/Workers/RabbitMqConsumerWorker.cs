
using OrderTracking.Worker.Consumers;

namespace OrderTracking.Worker.Workers;
public class RabbitMqConsumerWorker : BackgroundService
{
	private readonly IMessageConsumer _messageConsumer;

	public RabbitMqConsumerWorker(IMessageConsumer messageConsumer)
	{
		_messageConsumer = messageConsumer;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await _messageConsumer.StartConsumingAsync(stoppingToken);

		await Task.Delay(Timeout.Infinite, stoppingToken);
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		await _messageConsumer.StopConsumingAsync();

		await base.StopAsync(cancellationToken);
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}