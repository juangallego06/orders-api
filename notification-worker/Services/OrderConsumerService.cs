using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using notification_worker.Configuration;
using notification_worker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace notification_worker.Services
{
    public class OrderConsumerService : BackgroundService
    {
        private readonly ILogger<OrderConsumerService> _logger;
        private readonly RabbitMQSettings _settings;
        private IConnection? _connection;
        private IModel? _channel;

        public OrderConsumerService(
            ILogger<OrderConsumerService> logger,
            IOptions<RabbitMQSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔧 Iniciando OrderConsumerService...");

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    DispatchConsumersAsync = true
                };

                _logger.LogInformation("🔗 Conectando a RabbitMQ en {HostName}:{Port}...", _settings.HostName, _settings.Port);

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar cola (asegurar que existe)
                _channel.QueueDeclare(
                    queue: _settings.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("✅ Cola declarada: {QueueName}", _settings.QueueName);

                // Configurar QoS (Quality of Service)
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation("👂 Iniciando consumo de mensajes de la cola '{QueueName}'...", _settings.QueueName);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("📨 [INFO] Mensaje recibido de RabbitMQ");
                        _logger.LogInformation("📦 Contenido: {Message}", message);

                        var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message);

                        if (orderMessage != null)
                        {
                            _logger.LogInformation("🔍 [INFO] Procesando pedido ID={OrderId}", orderMessage.OrderId);

                            // Simular procesamiento
                            await ProcessOrderAsync(orderMessage, stoppingToken);

                            _logger.LogInformation("✅ [INFO] Pedido procesado correctamente ID={OrderId}", orderMessage.OrderId);

                            // Acknowledge message
                            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ No se pudo deserializar el mensaje");
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error al procesar mensaje");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                _channel.BasicConsume(
                    queue: _settings.QueueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("🎉 OrderConsumerService iniciado y esperando mensajes...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al iniciar OrderConsumerService");
                throw;
            }

            return Task.CompletedTask;
        }

        private async Task ProcessOrderAsync(OrderMessage order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("📋 Detalle del pedido:");
            _logger.LogInformation("   - ID: {OrderId}", order.OrderId);
            _logger.LogInformation("   - Cliente: {CustomerName}", order.CustomerName);
            _logger.LogInformation("   - Producto: {Product}", order.Product);
            _logger.LogInformation("   - Cantidad: {Quantity}", order.Quantity);
            _logger.LogInformation("   - Fecha: {CreatedAt:yyyy-MM-dd HH:mm:ss}", order.CreatedAt);
            _logger.LogInformation("   - Estado: {Status}", order.Status);

            // Simular procesamiento
            await Task.Delay(1000, cancellationToken);

            // Aquí iría la lógica real de procesamiento
            // - Enviar email de confirmación
            // - Actualizar inventario
            // - Notificar a otros sistemas
            // - Guardar en base de datos

            _logger.LogInformation("📧 Simulando envío de email a cliente: {CustomerName}", order.CustomerName);
            _logger.LogInformation("📦 Simulando actualización de inventario para producto: {Product}", order.Product);
        }

        public override void Dispose()
        {
            _logger.LogInformation("🔌 Deteniendo OrderConsumerService...");
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
