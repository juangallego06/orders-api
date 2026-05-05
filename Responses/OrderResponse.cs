namespace Orders.API.Responses
{
    public class OrderResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string OrderId { get; set; }

        public OrderResponse()
        {
            Status = "accepted";
            Message = "Pedido recibido y enviado a RabbitMQ";
            OrderId = "ORD-1001";
        }
    }
}
