using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proiect.DTOs;
using Proiect.Services.Interfaces;
using System.Security.Claims;

namespace Proiect.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemActionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _orderService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);
            return Ok();
        }

        [HttpPost("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromQuery] int productId, [FromQuery] bool increase)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _orderService.UpdateQuantityAsync(userId, productId, increase);
            return Ok();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _orderService.ClearCartAsync(userId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartDisplay = await _orderService.GetCartForDisplayAsync(userId);
            return Ok(cartDisplay);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilizatorul nu este autentificat.");
            }

            try
            {
                var response = await _orderService.PlaceOrderAsync(userId, dto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return BadRequest("Session ID lipseste.");
            }

            try
            {
                bool isConfirmed = await _orderService.ConfirmPaymentAsync(session_id);

                if (isConfirmed)
                {
                    return Ok(new { message = "Plata a fost confirmata cu succes!" });
                }

                return BadRequest(new { message = "Plata nu a putut fi confirmată." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
