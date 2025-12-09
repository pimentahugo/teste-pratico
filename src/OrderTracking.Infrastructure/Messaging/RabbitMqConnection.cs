using OrderTracking.Infrastructure.Messaging.Settings;
using RabbitMQ.Client;

namespace OrderTracking.Infrastructure.Messaging;
public class RabbitMqConnection : IAsyncDisposable
{
	private readonly RabbitMqSettings _settings;

	public RabbitMqConnection(RabbitMqSettings settings)
	{
		_settings = settings;
	}

	private IConnection? _connection;

	public async Task<IConnection> GetConnection()
	{
		if (_connection?.IsOpen != true)
		{
			var factory = new ConnectionFactory()
			{
				HostName = _settings.HostName,
				UserName = _settings.UserName,
				Password = _settings.Password,
				Port = _settings.Port
			};

			_connection = await factory.CreateConnectionAsync();
		}

		return _connection;
	}
	public async ValueTask DisposeAsync()
	{
		if (_connection != null)
		{
			await _connection.CloseAsync();
			await _connection.DisposeAsync();
			_connection = null;
		}
	}
}