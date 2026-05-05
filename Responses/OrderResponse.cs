namespace Orders.API.Responses
{
    public class OrderResponse
    {
        public string Status { get; set; } = "accepted";
        public string Message { get; set; } = "Pedido recibido y enviado a RabbitMQ";
        public string OrderId { get; set; } = string.Empty;
    }
}
