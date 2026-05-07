using System.ComponentModel.DataAnnotations;

namespace Orders.API.Requests
{
    public class OrderRequest
    {
        public required string CustomerName { get; set; }
        public required string Product { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un número válido mayor o igual a 0")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; } = 85000;
    }
}
