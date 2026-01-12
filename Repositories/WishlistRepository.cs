using Microsoft.EntityFrameworkCore;
using Proiect.Context;
using Proiect.Models;
using Proiect.Repositories.Interfaces;

namespace Proiect.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private ShopContext _context;
        public WishlistRepository(ShopContext shopContext)
        {
            _context = shopContext;
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FindAsync(productId);
        }

        public async Task CreateWishlistAsync(Wishlist wishlist)
        {
            await _context.Wishlists.AddAsync(wishlist);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Wishlist> GetByUserIdAsync(string userId)
        {
            return await _context.Wishlists
                .Include(w => w.Products)
                .ThenInclude(p => p.Category)
                .Include(w => w.Products)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }
    }
}
