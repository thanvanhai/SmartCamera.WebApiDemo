using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCamera.WebApiDemo.Messaging
{
    public class Producer : IMessageProducer, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<Producer>? _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;

        private Producer(IConnection connection, IChannel channel, ILogger<Producer>? logger = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public static async Task<Producer> CreateAsync(string amqpUrl, ILogger<Producer>? logger = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(amqpUrl))
                throw new ArgumentException("AMQP URL cannot be null or empty.", nameof(amqpUrl));

            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(amqpUrl),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    AutomaticRecoveryEnabled = true,
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };

                logger?.LogInformation("Attempting to connect to RabbitMQ at {AmqpUrl}...", amqpUrl);

                var connection = await factory.CreateConnectionAsync(cancellationToken);
                // IMPORTANT: use named parameter so the cancellation token is passed to the correct arg
                var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                logger?.LogInformation("Successfully connected to RabbitMQ and created a channel.");
                return new Producer(connection, channel, logger);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to create RabbitMQ Producer connection.");
                throw;
            }
        }

        public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true && !_disposed;

        public async Task PublishAsync(string exchange, string routingKey, object message, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Producer));

            if (string.IsNullOrWhiteSpace(exchange))
                throw new ArgumentException("Exchange cannot be null or empty.", nameof(exchange));
            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("Routing key cannot be null or empty.", nameof(routingKey));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                // Ensure the exchange exists (idempotent)
                await _channel.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                var jsonString = JsonSerializer.Serialize(message, _jsonOptions);
                var body = Encoding.UTF8.GetBytes(jsonString);

                // Create basic properties via BasicProperties (7.x)
                var properties = new BasicProperties
                {
                    DeliveryMode = DeliveryModes.Persistent,
                    ContentType = "application/json",
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                await _channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body,
                    mandatory: false,
                    cancellationToken: cancellationToken);

                _logger?.LogDebug("Message published successfully to exchange '{Exchange}' with routing key '{RoutingKey}'.", exchange, routingKey);
            }
            catch (AlreadyClosedException ex)
            {
                _logger?.LogError(ex, "Failed to publish message because the connection or channel is closed.");
                throw new InvalidOperationException("RabbitMQ connection is closed.", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An unexpected error occurred while publishing a message to exchange '{Exchange}'.", exchange);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            _logger?.LogInformation("Disposing RabbitMQ Producer...");

            try
            {
                if (_channel?.IsOpen == true)
                {
                    await _channel.CloseAsync();
                    _logger?.LogDebug("RabbitMQ channel closed.");
                }
                if (_connection?.IsOpen == true)
                {
                    await _connection.CloseAsync();
                    _logger?.LogDebug("RabbitMQ connection closed.");
                }
                _logger?.LogInformation("RabbitMQ Producer disposed successfully.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while disposing the RabbitMQ Producer.");
            }
        }
    }
}
