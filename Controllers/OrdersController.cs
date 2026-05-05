using Microsoft.AspNetCore.Mvc;
using Orders.API.Requests;
using Orders.API.Responses;

namespace Orders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(OrderRequest orderRequest)
        {
            if (orderRequest == null) return BadRequest(new { error = "Petición invalida" });
            if (string.IsNullOrEmpty(orderRequest.CustomerName)) return BadRequest(new { error = "El nombre es obligatorio" });
            if (string.IsNullOrEmpty(orderRequest.Product)) return BadRequest(new { error = "El producto es obligatorio" });
            if (orderRequest.Quantity < 0) return BadRequest(new { error = "Cantidad invalida" });

            return Ok(new OrderResponse());
        }
    }
}
