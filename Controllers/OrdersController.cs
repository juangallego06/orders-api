using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orders.API.Models;
using Orders.API.Requests;
using Orders.API.Responses;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IRabbitMQProducer rabbitMQProducer,
            ILogger<OrdersController> logger)
        {
            _rabbitMQProducer = rabbitMQProducer;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderRequest orderRequest)
        {
            try
            {
                _logger.LogInformation("📥 Nuevo pedido recibido: {CustomerName} - {Product} x{Quantity}",
                    orderRequest.CustomerName, orderRequest.Product, orderRequest.Quantity);

                // Validaciones
                if (orderRequest == null)
                {
                    _logger.LogWarning("⚠️ Petición inválida: OrderRequest es null");
                    return BadRequest(new { error = "Petición invalida" });
                }

                if (string.IsNullOrEmpty(orderRequest.CustomerName))
                {
                    _logger.LogWarning("⚠️ Validación fallida: CustomerName está vacío");
                    return BadRequest(new { error = "El nombre es obligatorio" });
                }

                if (string.IsNullOrEmpty(orderRequest.Product))
                {
                    _logger.LogWarning("⚠️ Validación fallida: Product está vacío");
                    return BadRequest(new { error = "El producto es obligatorio" });
                }

                if (orderRequest.Quantity < 0)
                {
                    _logger.LogWarning("⚠️ Validación fallida: Quantity es negativa");
                    return BadRequest(new { error = "Cantidad invalida" });
                }

                // Crear orden con ID único
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerName = orderRequest.CustomerName,
                    Product = orderRequest.Product,
                    Quantity = orderRequest.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    Status = "pending"
                };

                _logger.LogInformation("📋 Orden creada con ID: {OrderId}", order.Id);

                // Crear mensaje para RabbitMQ
                var orderMessage = new OrderMessage
                {
                    OrderId = order.Id,
                    CustomerName = order.CustomerName,
                    Product = order.Product,
                    Quantity = order.Quantity,
                    CreatedAt = order.CreatedAt,
                    Status = order.Status
                };

                // Publicar en RabbitMQ
                await _rabbitMQProducer.PublishOrderAsync(orderMessage);

                _logger.LogInformation("✅ Pedido procesado exitosamente. ID={OrderId}", order.Id);

                var response = new OrderResponse
                {
                    Status = "accepted",
                    Message = "Pedido recibido y enviado a RabbitMQ",
                    OrderId = $"ORD-{order.Id.ToString().Substring(0, 8).ToUpper()}"
                };

                return Accepted(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al procesar el pedido");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}
