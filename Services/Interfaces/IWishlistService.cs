using Proiect.DTOs;

namespace Proiect.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<bool> AddProductToUserWishlistAsync(string userId, int productId);
        Task<bool> RemoveProductFromUserWishlistAsync(string userId, int productId);
        Task<List<ProductDto>> GetUserWishlistProductsAsync(string userId);
    }
}
