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
	private IChannel? _channel;
	private bool _disposed;

	public RabbitMqConsumer(IConnection connection, RabbitMqSettings settings, IServiceProvider serviceProvider)
	{
		_connection = connection;
		_settings = settings;
		_serviceProvider = serviceProvider;
	}

	public async Task StartConsumingAsync(CancellationToken cancellationToken)
	{
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

		using var scope = _serviceProvider.CreateScope();

		var body = eventArgs.Body.ToArray();
		var messageJson = Encoding.UTF8.GetString(body);

		var orderEvent = JsonSerializer.Deserialize<PedidoCriadoEvent>(messageJson);

		if(orderEvent is null)
		{
			if(_channel is not null)
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

		if(_channel is not null)
		{
			await _channel.BasicAckAsync(
				deliveryTag: eventArgs.DeliveryTag,
				multiple: false,
				cancellationToken: cancellationToken);
		}
	}

	public async Task StopConsumingAsync()
	{
		if (_channel is not null)
		{
			await _channel.CloseAsync();
			_channel.Dispose();
			_channel = null;
		}
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