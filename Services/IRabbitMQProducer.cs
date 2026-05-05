using Orders.API.Models;

namespace Orders.API.Services
{
    public interface IRabbitMQProducer
    {
        Task PublishOrderAsync(OrderMessage order);
        void Connect();
        void Disconnect();
    }
}
