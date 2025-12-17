using OrderTracking.Infrastructure.Messaging.Events;
using OrderTracking.Infrastructure.Messaging.Settings;
using OrderTracking.Worker.Handlers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderTracking.Worker.Consumers;

public class RabbitMqConsumer : IMessageConsumer, IAsyncDisposable
{
	private readonly IConnection _connection;
	private readonly RabbitMqSettings _settings;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<RabbitMqConsumer> _logger;
	private IChannel? _channel;
	private bool _disposed;

	public RabbitMqConsumer(IConnection connection, RabbitMqSettings settings, IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger)
	{
		_connection = connection;
		_settings = settings;
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	public async Task StartConsumingAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Criando canal RabbitMQ para consumir fila {QueueName}",_settings.QueueName);

		_channel = await _connection.CreateChannelAsync();

		await _channel.BasicQosAsync(
			prefetchSize: 0,
			prefetchCount: 1,
			global: false,
			cancellationToken: cancellationToken);

		await _channel.QueueDeclareAsync(
			queue: _settings.QueueName,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null,
			cancellationToken: cancellationToken);

		await _channel.BasicQosAsync(0, 1, false);

		_logger.LogInformation(
				"Configurando consumer para fila {QueueName}",
				_settings.QueueName);

		var consumer = new AsyncEventingBasicConsumer(_channel);

		consumer.ReceivedAsync += async (sender, eventArgs) =>
		{
			await ProcessMessageAsync(eventArgs, cancellationToken);
		};

		await _channel.BasicConsumeAsync(
			queue: _settings.QueueName,
			autoAck: false,
			consumer: consumer,
			cancellationToken: cancellationToken);
	}

	private async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
	{
		var messageId = eventArgs.BasicProperties?.MessageId ?? "unknown";

		try
		{
			using var scope = _serviceProvider.CreateScope();

			var body = eventArgs.Body.ToArray();
			var messageJson = Encoding.UTF8.GetString(body);

			_logger.LogInformation(
						"Mensagem recebida. MessageId: {MessageId}, Size: {MessageSize} bytes",
						messageId,
						body.Length
					);

			var orderEvent = JsonSerializer.Deserialize<PedidoCriadoEvent>(messageJson);

			if (orderEvent is null)
			{
				_logger.LogError(
							"Falha ao deserializar mensagem {MessageId}. Payload: {Payload}",
							messageId,
							messageJson
						);

				if (_channel is not null)
				{
					await _channel.BasicNackAsync(
						deliveryTag: eventArgs.DeliveryTag,
						multiple: false,
						requeue: false,
						cancellationToken: cancellationToken);
				}
				return;
			}

			var handler = scope.ServiceProvider.GetRequiredService<IOrderEventHandler>();
			await handler.HandleAsync(orderEvent, cancellationToken);

			if (_channel is not null)
			{
				await _channel.BasicAckAsync(
					deliveryTag: eventArgs.DeliveryTag,
					multiple: false,
					cancellationToken: cancellationToken);
			}

			_logger.LogInformation(
						"Mensagem {MessageId} processada com sucesso",
						messageId
					);
		}
		catch (Exception ex)
		{
			_logger.LogError(
						ex,
						"Erro ao processar mensagem {MessageId}. Erro: {ErrorMessage}",
						messageId,
						ex.Message
					);

			if (_channel is not null)
				await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, true);
		}
	}

	public async Task StopConsumingAsync()
	{
		_logger.LogInformation("Parando consumidor RabbitMQ...");

		if (_channel is not null)
		{
			await _channel.CloseAsync();
			_channel.Dispose();
			_channel = null;
		}

		_logger.LogInformation("Consumidor RabbitMQ parado");
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;

		if (_channel is not null)
		{
			await _channel.DisposeAsync();
		}

		_disposed = true;

		GC.SuppressFinalize(this);
	}
}