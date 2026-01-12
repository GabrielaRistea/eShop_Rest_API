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
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }
        [HttpPost("add/{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            bool wasAdded = await _wishlistService.AddProductToUserWishlistAsync(userId, productId);

            if (wasAdded == false)
            {
                return BadRequest(new { message = "Acest produs este deja in wishlist-ul tau!" });
            }

            return Ok(new { message = "Produsul a fost adaugat cu succes." });
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            bool isDeleted = await _wishlistService.RemoveProductFromUserWishlistAsync(userId, productId);

            if (isDeleted == false)
            {
                return NotFound(new { message = "Produsul nu a fost gasit in wishlist-ul tau." });
            }

            return Ok(new { message = "Produsul a fost eliminat cu succes." });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWishlist()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }
            Console.WriteLine($"ID-ul utilizatorului este: {userId}");
            List<ProductDto> myProducts = await _wishlistService.GetUserWishlistProductsAsync(userId);

            return Ok(myProducts);
        }
    }
}
