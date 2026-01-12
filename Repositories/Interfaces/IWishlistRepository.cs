using Proiect.Models;

namespace Proiect.Repositories.Interfaces
{
    public interface IWishlistRepository
    {
        Task<Wishlist> GetByUserIdAsync(string userId);
        Task<Product> GetProductByIdAsync(int productId);
        Task CreateWishlistAsync(Wishlist wishlist);
        Task SaveChangesAsync();
    }
}
