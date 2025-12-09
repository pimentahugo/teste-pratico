using OrderTracking.Domain.Interfaces;
using OrderTracking.Infrastructure.Messaging.Settings;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderTracking.Infrastructure.Messaging;
public class RabbitMqPublisher : IMessagePublisher
{
	private readonly RabbitMqSettings _settings;
	private readonly IConnection _connection;

	public RabbitMqPublisher(RabbitMqSettings settings, IConnection connection)
	{
		_settings = settings;
		_connection = connection;
	}

	public async Task PublishAsync<T>(T message) where T : class
	{
		using var channel = await _connection.CreateChannelAsync();

		await channel.QueueDeclareAsync(
			queue: _settings.QueueName,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null);

		var json = JsonSerializer.Serialize(message);
		var body = Encoding.UTF8.GetBytes(json);

		var properties = new BasicProperties
		{
			Persistent = true
		};

		await channel.BasicPublishAsync(
			exchange: string.Empty,
			routingKey: _settings.QueueName,
			mandatory: false,
			basicProperties: properties,
			body: body);
	}
}