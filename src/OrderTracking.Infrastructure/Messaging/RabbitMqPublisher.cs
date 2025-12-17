using Microsoft.Extensions.Logging;
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
	private readonly ILogger<RabbitMqPublisher> _logger;

	public RabbitMqPublisher(
		RabbitMqSettings settings,
		IConnection connection,
		ILogger<RabbitMqPublisher> logger)
	{
		_settings = settings;
		_connection = connection;
		_logger = logger;
	}

	public async Task PublishAsync<T>(T message) where T : class
	{
		try
		{
			_logger.LogDebug(
				"Iniciando publicação de mensagem do tipo {MessageType} na fila {QueueName}",
				typeof(T).Name,
				_settings.QueueName);

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
				Persistent = true,
				MessageId = Guid.NewGuid().ToString(),
				Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
				ContentType = "application/json"
			};

			_logger.LogInformation(
				"Publicando mensagem {MessageId} na fila {QueueName}. Tamanho: {MessageSize} bytes",
				properties.MessageId,
				_settings.QueueName,
				body.Length);

			await channel.BasicPublishAsync(
				exchange: string.Empty,
				routingKey: _settings.QueueName,
				mandatory: false,
				basicProperties: properties,
				body: body);

			_logger.LogInformation(
				"Mensagem {MessageId} publicada com sucesso na fila {QueueName}",
				properties.MessageId,
				_settings.QueueName);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Erro ao publicar mensagem do tipo {MessageType} na fila {QueueName}",
				typeof(T).Name,
				_settings.QueueName);

			throw;
		}
	}
}