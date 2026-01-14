using Microsoft.EntityFrameworkCore;
using Proiect.Context;
using Proiect.Models;
using Proiect.Repositories.Interfaces;

namespace Proiect.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private ShopContext _context;
        public OrderRepository(ShopContext shopContext)
        {
            _context = shopContext;
        }

        public async Task<Order> GetActiveCartAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.IdUser == userId && o.statusOrder == "Cart");
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FindAsync(productId);
        }

        public async Task CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderItemAsync(OrderItem item)
        {
            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
        }

        public async Task<HistoryOrders> GetHistoryByUserIdAsync(string userId)
        {
            return await _context.HistoryOrders
                .FirstOrDefaultAsync(h => h.IdUser == userId);
        }

        public async Task CreateHistoryAsync(HistoryOrders history)
        {
            _context.HistoryOrders.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Where(o => o.statusOrder != "Cart")
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.IdUser == userId && o.statusOrder != "Cart")
                .ToListAsync();
        }
    }
}
