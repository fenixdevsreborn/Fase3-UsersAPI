using RabbitMQ.Client;
using System.Text.Json;

namespace ms_users.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message) where T : class;
}

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private IChannel? _channel;
    private readonly IConfiguration _configuration;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        var factory = new ConnectionFactory
        {
            HostName = RequireConfiguration("RabbitMq:Host"),
            Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672"),
            UserName = RequireConfiguration("RabbitMq:Username"),
            Password = RequireConfiguration("RabbitMq:Password"),
            VirtualHost = RequireConfiguration("RabbitMq:VirtualHost")
            // DispatchConsumersAsync is removed - async is default in v13.x+
        };

        try
        {
            _connection = factory.CreateConnectionAsync().Result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to connect to RabbitMQ", ex);
        }
    }

    private string RequireConfiguration(string key)
    {
        return _configuration[key]
            ?? throw new InvalidOperationException($"RabbitMQ configuration '{key}' is not configured");
    }

    private async Task EnsureChannelAsync()
    {
        if (_channel == null || _channel.IsClosed)
        {
            _channel = await _connection.CreateChannelAsync();
        }
    }

    public async Task PublishAsync<T>(string queueName, T message) where T : class
    {
        try
        {
            await EnsureChannelAsync();

            // Declare queue (idempotent - won't fail if it exists)
            await _channel!.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var exchangeName = _configuration["RabbitMq:ExchangeName"];
            if (!string.IsNullOrWhiteSpace(exchangeName))
            {
                await _channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null
                );

                await _channel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: queueName
                );
            }

            var messageBody = JsonSerializer.Serialize(message);
            var body = System.Text.Encoding.UTF8.GetBytes(messageBody);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel.BasicPublishAsync(
                exchange: exchangeName ?? string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to publish message to queue '{queueName}'", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null && _channel.IsOpen)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection != null && _connection.IsOpen)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}
