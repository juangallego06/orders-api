using System.Text.Json.Serialization;

namespace notification_worker.Models
{
    public class OrderMessage
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("product")]
        public string Product { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";
    }
}
