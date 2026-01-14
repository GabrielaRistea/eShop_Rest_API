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
    public class HistoryOrdersController : Controller
    {
        private readonly IOrderService _orderService;
        public HistoryOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync();
            return Ok(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, dto.NewStatus);

            if (!success)
            {
                return NotFound(new { message = "Comanda nu a fost găsita." });
            }

            return Ok(new { message = "Statusul a fost actualizat cu succes!" });
        }
    }
}
