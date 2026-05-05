using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orders.API.Configuration;
using Orders.API.Models;
using RabbitMQ.Client;

namespace Orders.API.Services
{
    public class RabbitMQProducer : IRabbitMQProducer, IDisposable
    {
        private readonly ILogger<RabbitMQProducer> _logger;
        private readonly RabbitMQSettings _settings;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQProducer(
            ILogger<RabbitMQProducer> logger,
            IOptions<RabbitMQSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public void Connect()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password
                };

                _logger.LogInformation("🔗 Conectando a RabbitMQ en {HostName}:{Port}...", _settings.HostName, _settings.Port);

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchange
                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("✅ Exchange declarado: {ExchangeName}", _settings.ExchangeName);

                // Declarar cola
                _channel.QueueDeclare(
                    queue: _settings.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("✅ Cola declarada: {QueueName}", _settings.QueueName);

                // Binding entre exchange y cola
                _channel.QueueBind(
                    queue: _settings.QueueName,
                    exchange: _settings.ExchangeName,
                    routingKey: _settings.RoutingKey);

                _logger.LogInformation("✅ Binding creado: {QueueName} -> {ExchangeName} con RK: {RoutingKey}",
                    _settings.QueueName, _settings.ExchangeName, _settings.RoutingKey);

                _logger.LogInformation("🎉 Conexión a RabbitMQ establecida exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al conectar a RabbitMQ");
                throw;
            }
        }

        public async Task PublishOrderAsync(OrderMessage order)
        {
            try
            {
                if (_channel == null || !_channel.IsOpen)
                {
                    _logger.LogWarning("⚠️ Canal no está abierto, reconectando...");
                    Connect();
                }

                var message = JsonSerializer.Serialize(order);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel!.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = order.OrderId.ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: _settings.RoutingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("📤 [INFO] Mensaje enviado a RabbitMQ: Pedido creado ID={OrderId}", order.OrderId);
                _logger.LogInformation("📦 Contenido del mensaje: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al publicar mensaje en RabbitMQ");
                throw;
            }

            await Task.CompletedTask;
        }

        public void Disconnect()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _logger.LogInformation("🔌 Desconectado de RabbitMQ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al desconectar de RabbitMQ");
            }
        }

        public void Dispose()
        {
            Disconnect();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
