using Microsoft.Extensions.Logging;
using OrderTracking.Infrastructure.Messaging.Settings;
using RabbitMQ.Client;

namespace OrderTracking.Infrastructure.Messaging;

public class RabbitMqConnection : IAsyncDisposable
{
	private readonly RabbitMqSettings _settings;
	private readonly ILogger<RabbitMqConnection> _logger;
	private IConnection? _connection;

	public RabbitMqConnection(
		RabbitMqSettings settings,
		ILogger<RabbitMqConnection> logger)
	{
		_settings = settings;
		_logger = logger;
	}

	public async Task<IConnection> GetConnection()
	{
		if (_connection?.IsOpen != true)
		{
			try
			{
				_logger.LogInformation(
					"Conectando ao RabbitMQ. Host: {HostName}, Port: {Port}, User: {UserName}",
					_settings.HostName,
					_settings.Port,
					_settings.UserName);

				var factory = new ConnectionFactory()
				{
					HostName = _settings.HostName,
					UserName = _settings.UserName,
					Password = _settings.Password,
					Port = _settings.Port
				};

				_connection = await factory.CreateConnectionAsync();

				_logger.LogInformation(
					"Conexão com RabbitMQ estabelecida com sucesso. Host: {HostName}",
					_settings.HostName);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Falha ao conectar com RabbitMQ. Host: {HostName}, Port: {Port}",
					_settings.HostName,
					_settings.Port);

				throw;
			}
		}

		return _connection;
	}

	public async ValueTask DisposeAsync()
	{
		if (_connection != null)
		{
			_logger.LogInformation("Fechando conexão com RabbitMQ");

			await _connection.CloseAsync();
			await _connection.DisposeAsync();
			_connection = null;

			_logger.LogInformation("Conexão com RabbitMQ fechada");
		}
	}
}