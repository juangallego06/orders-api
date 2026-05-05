namespace Orders.API.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "pending";
    }
}
