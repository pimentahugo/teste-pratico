namespace OrderTracking.Domain.Interfaces;
public interface IMessagePublisher
{
	Task PublishAsync<T>(T message) where T : class;
}