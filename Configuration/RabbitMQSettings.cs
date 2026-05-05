namespace Orders.API.Configuration
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ExchangeName { get; set; } = "orders.exchange";
        public string QueueName { get; set; } = "pedidos";
        public string RoutingKey { get; set; } = "order.created";
    }
}
