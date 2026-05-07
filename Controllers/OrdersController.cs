using Microsoft.AspNetCore.Mvc;
using Orders.API.Requests;
using Orders.API.Responses;
using RabbitMQ.Client;
using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace Orders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IConfiguration config, ILogger<OrdersController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(OrderRequest orderRequest)
        {
            if (orderRequest == null)
                return BadRequest(new { error = "Petición invalida" });
            if (string.IsNullOrEmpty(orderRequest.CustomerName))
                return BadRequest(new { error = "El nombre es obligatorio" });
            if (string.IsNullOrEmpty(orderRequest.Product))
                return BadRequest(new { error = "El producto es obligatorio" });
            if (orderRequest.Quantity < 0)
                return BadRequest(new { error = "Cantidad invalida" });

            var orderId = $"ORD-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            PublishToRabbitMQ(orderId, orderRequest);
            await PublishToKafkaAsync(orderId, orderRequest);

            return Ok(new OrderResponse
            {
                Status  = "accepted",
                Message = "Pedido recibido, enviado a RabbitMQ y evento OrderCreated publicado en Kafka",
                OrderId = orderId
            });
        }

        private void PublishToRabbitMQ(string orderId, OrderRequest orderRequest)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["RABBITMQ_HOST"] ?? "rabbitmq",
                    UserName = _config["RABBITMQ_USER"] ?? "admin",
                    Password = _config["RABBITMQ_PASS"] ?? "admin"
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "pedidos", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var message = new
                {
                    type         = "OrderCreatedMessage",
                    orderId,
                    customerName = orderRequest.CustomerName,
                    product      = orderRequest.Product,
                    quantity     = orderRequest.Quantity,
                    createdAt    = DateTime.UtcNow.ToString("o")
                };

                var props = channel.CreateBasicProperties();
                props.Persistent = true;

                channel.BasicPublish(
                    exchange: "",
                    routingKey: "pedidos",
                    basicProperties: props,
                    body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message))
                );

                _logger.LogInformation("[RabbitMQ] Mensaje publicado en cola 'pedidos' - OrderId: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] Error al publicar - OrderId: {OrderId}", orderId);
            }
        }

        private async Task PublishToKafkaAsync(string orderId, OrderRequest orderRequest)
        {
            try
            {
                var broker = _config["KAFKA_BROKER"] ?? "kafka:9092";
                var topic  = _config["KAFKA_TOPIC"]  ?? "orders.events";

                var kafkaEvent = new
                {
                    eventId       = Guid.NewGuid().ToString(),
                    eventType     = "OrderCreated",
                    eventVersion  = "1.0",
                    occurredAt    = DateTime.UtcNow.ToString("o"),
                    source        = "orders-api",
                    correlationId = Guid.NewGuid().ToString(),
                    data = new
                    {
                        orderId,
                        customerName = orderRequest.CustomerName,
                        product      = orderRequest.Product,
                        quantity     = orderRequest.Quantity,
                        unitPrice    = orderRequest.UnitPrice,
                        total        = orderRequest.Quantity * orderRequest.UnitPrice
                    }
                };

                var producerConfig = new ProducerConfig { BootstrapServers = broker };
                using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

                var result = await producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key   = orderId,
                    Value = JsonSerializer.Serialize(kafkaEvent)
                });

                _logger.LogInformation("[Kafka] Evento OrderCreated publicado - topic={Topic} partition={Partition} offset={Offset}",
                    result.Topic, result.Partition.Value, result.Offset.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Kafka] Error al publicar evento - OrderId: {OrderId}", orderId);
            }
        }
    }
}
