using Proiect.Models;

namespace Proiect.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetActiveCartAsync(string userId);
        Task<Product> GetProductByIdAsync(int productId);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task UpdateOrderAsync(Order order);
        Task CreateOrderAsync(Order order);
        Task DeleteOrderItemAsync(OrderItem item);
        Task UpdateProductAsync(Product product);
        Task<HistoryOrders> GetHistoryByUserIdAsync(string userId);
        Task CreateHistoryAsync(HistoryOrders history);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    }
}
