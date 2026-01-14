using Proiect.DTOs;
using Proiect.Models;

namespace Proiect.Services.Interfaces
{
    public interface IOrderService
    {
        Task AddToCartAsync(string userId, int productId, int quantity);
        Task UpdateQuantityAsync(string userId, int productId, bool increase);
        Task ClearCartAsync(string userId);
        Task<CartDisplayDto> GetCartForDisplayAsync(string userId);
        Task<bool> ConfirmPaymentAsync(string sessionId);
        Task<string> CreateStripeSession(Order order);
        Task<CheckoutResponseDto> PlaceOrderAsync(string userId, CheckoutDto dto);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<List<OrderAdminDto>> GetAllOrdersForAdminAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);
    }
}
