namespace OrderTracking.Worker.Consumers;
public interface IMessageConsumer
{
	Task StartConsumingAsync(CancellationToken cancellationToken);
	Task StopConsumingAsync();
}
