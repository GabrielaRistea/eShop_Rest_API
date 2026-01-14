using Proiect.DTOs;
using Proiect.Models;
using Proiect.Repositories.Interfaces;
using Proiect.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace Proiect.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IConfiguration _config;

        public OrderService(IOrderRepository orderRepository, IConfiguration config)
        {
            _orderRepository = orderRepository;
            _config = config;
        }

        private float CalculateTotal(Order cart)
        {
            float total = 0;
            foreach (var item in cart.OrderItems)
            {
                if (item.Product != null)
                {
                    total += item.Quantity * item.Product.Price;
                }
            }
            return total;
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await _orderRepository.GetActiveCartAsync(userId);
            var product = await _orderRepository.GetProductByIdAsync(productId);

            if (product == null) throw new Exception("Produsul nu exista.");

            if (cart == null)
            {
                cart = new Order();
                cart.IdUser = userId;
                cart.statusOrder = "Cart";
                cart.OrderDate = DateTime.Now.ToUniversalTime();
                cart.OrderItems = new List<OrderItem>();
                await _orderRepository.CreateOrderAsync(cart);
            }

            OrderItem existingItem = null;
            foreach (var item in cart.OrderItems)
            {
                if (item.ProductId == productId)
                {
                    existingItem = item;
                    break;
                }
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new OrderItem();
                newItem.ProductId = productId;
                newItem.Quantity = quantity;
                newItem.IdOrder = cart.OrderID;
                cart.OrderItems.Add(newItem);
            }

            cart.TotalAmount = CalculateTotal(cart);
            await _orderRepository.UpdateOrderAsync(cart);
        }

        public async Task UpdateQuantityAsync(string userId, int productId, bool increase)
        {
            var cart = await _orderRepository.GetActiveCartAsync(userId);
            if (cart == null) return;

            foreach (var item in cart.OrderItems)
            {
                if (item.ProductId == productId)
                {
                    if (increase) item.Quantity++;
                    else item.Quantity--;

                    if (item.Quantity <= 0)
                    {
                        await _orderRepository.DeleteOrderItemAsync(item);
                    }
                    break;
                }
            }

            var updatedCart = await _orderRepository.GetActiveCartAsync(userId);
            updatedCart.TotalAmount = CalculateTotal(updatedCart);
            await _orderRepository.UpdateOrderAsync(updatedCart);
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _orderRepository.GetActiveCartAsync(userId);
            if (cart != null)
            {
                foreach (var item in cart.OrderItems)
                {
                    await _orderRepository.DeleteOrderItemAsync(item);
                }
                cart.TotalAmount = 0;
                await _orderRepository.UpdateOrderAsync(cart);
            }
        }

        public async Task<CartDisplayDto> GetCartForDisplayAsync(string userId)
        {
            var cart = await _orderRepository.GetActiveCartAsync(userId);

            var result = new CartDisplayDto();
            result.Items = new List<CartItemDisplayDto>();
            result.TotalAmount = 0;

            if (cart == null || cart.OrderItems == null)
            {
                return result;
            }

            foreach (var item in cart.OrderItems)
            {
                var itemDto = new CartItemDisplayDto();

                itemDto.ProductId = item.ProductId;
                itemDto.Quantity = item.Quantity;

                if (item.Product != null)
                {
                    itemDto.ProductName = item.Product.Name;
                    itemDto.PricePerUnit = item.Product.Price;
                    itemDto.ProductImage = item.Product.ProductImage;
                    itemDto.Subtotal = item.Quantity * item.Product.Price;

                    result.TotalAmount += itemDto.Subtotal;
                }

                result.Items.Add(itemDto);
            }

            return result;
        }

        public async Task<CheckoutResponseDto> PlaceOrderAsync(string userId, CheckoutDto dto)
        {
            var cart = await _orderRepository.GetActiveCartAsync(userId);

            if (cart == null)
            {
                throw new Exception("Nu aveti niciun cos activ.");
            }

            if (cart.OrderItems.Count == 0)
            {
                throw new Exception("Cosul este gol.");
            }

            foreach (var item in cart.OrderItems)
            {
                var product = item.Product;
                if (product == null) throw new Exception("Unul dintre produse nu mai exista.");

                if (product.Stock < item.Quantity)
                {
                    throw new Exception("Stoc insuficient pentru produsul: " + product.Name +
                                        ". In stoc: " + product.Stock + ", cerut: " + item.Quantity);
                }
            }
            float finalTotal = 0;
            foreach (var item in cart.OrderItems)
            {
                finalTotal += item.Quantity * item.Product.Price;
            }
            cart.TotalAmount = finalTotal;
            cart.Address = dto.ShippingAddress;
            cart.PhoneNumber = dto.PhoneNumber;
            cart.PaymentMethod = dto.PaymentMethod;
            cart.OrderDate = DateTime.Now.ToUniversalTime();

            var response = new CheckoutResponseDto();

            if (dto.PaymentMethod == "Ramburs")
            {
                cart.statusOrder = "Plasata_Ramburs";

                foreach (var item in cart.OrderItems)
                {
                    var product = item.Product;
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        await _orderRepository.UpdateProductAsync(product);
                    }
                }
                await AssignOrderToHistory(userId, cart);
                await _orderRepository.UpdateOrderAsync(cart);

                response.RedirectToStripe = false;
                response.OrderId = cart.OrderID;
                return response;
            }
            else if (dto.PaymentMethod == "card")
            {
                cart.statusOrder = "Plata in asteptare";
                await _orderRepository.UpdateOrderAsync(cart);

                string stripeUrl = await CreateStripeSession(cart);

                response.RedirectToStripe = true;
                response.StripeUrl = stripeUrl;
                response.OrderId = cart.OrderID;
                return response;
            }
            else
            {
                throw new Exception("Metoda de plata invalida. Va rugam alegeti 'card' sau 'Ramburs'.");
            }
        }

        public async Task<string> CreateStripeSession(Order order)
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in order.OrderItems)
            {
                var sessionItem = new SessionLineItemOptions();

                long unitAmount = (long)(item.Product.Price * 100);

                sessionItem.PriceData = new SessionLineItemPriceDataOptions();
                sessionItem.PriceData.UnitAmount = unitAmount;
                sessionItem.PriceData.Currency = "ron";

                sessionItem.PriceData.ProductData = new SessionLineItemPriceDataProductDataOptions();
                sessionItem.PriceData.ProductData.Name = item.Product.Name;

                sessionItem.Quantity = item.Quantity;

                lineItems.Add(sessionItem);
            }

            var options = new SessionCreateOptions();
            options.PaymentMethodTypes = new List<string> { "card" };
            options.LineItems = lineItems;
            options.Mode = "payment";

            options.SuccessUrl = "http://localhost:4200/order-success?session_id={CHECKOUT_SESSION_ID}";
            options.CancelUrl = "http://localhost:4200/cart";

            options.Metadata = new Dictionary<string, string>();
            options.Metadata.Add("OrderId", order.OrderID.ToString());

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Url;
        }

        public async Task<bool> ConfirmPaymentAsync(string sessionId)
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var service = new SessionService();
            Session session = await service.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                string orderIdStr = session.Metadata["OrderId"];
                int orderId = int.Parse(orderIdStr);

                var order = await _orderRepository.GetOrderByIdAsync(orderId);

                if (order != null && order.statusOrder == "Plata in asteptare")
                {
                    order.statusOrder = "Platit_Stripe";

                    foreach (var item in order.OrderItems)
                    {
                        var product = item.Product;
                        product.Stock -= item.Quantity;
                        await _orderRepository.UpdateProductAsync(product);
                    }
                    await AssignOrderToHistory(order.IdUser, order);
                    await _orderRepository.UpdateOrderAsync(order);
                    return true;
                }
            }
            return false;
        }

        private async Task AssignOrderToHistory(string userId, Order order)
        {
            var history = await _orderRepository.GetHistoryByUserIdAsync(userId);

            if (history == null)
            {
                history = new HistoryOrders();
                history.IdUser = userId;
                await _orderRepository.CreateHistoryAsync(history);
            }

            order.IdHistoryOrders = history.Id;
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            var allOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);

            var history = new List<Order>();
            foreach (var o in allOrders)
            {
                if (o.statusOrder != "Cart")
                {
                    history.Add(o);
                }
            }
            return history;
        }

        public async Task<List<OrderAdminDto>> GetAllOrdersForAdminAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            var result = new List<OrderAdminDto>();

            foreach (var o in orders)
            {
                var dto = new OrderAdminDto
                {
                    OrderId = o.OrderID,
                    OrderDate = o.OrderDate,
                    Status = o.statusOrder,
                    TotalAmount = o.TotalAmount,
                    Address = o.Address,
                    PhoneNumber = o.PhoneNumber,
                    CustomerName = o.User != null ? o.User.UserName : "Anonim",
                    CustomerEmail = o.User != null ? o.User.Email : "-"
                };
                result.Add(dto);
            }
            return result;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);

            if (order == null) return false;

            order.statusOrder = newStatus;
            await _orderRepository.UpdateOrderAsync(order);

            return true;
        }
    }
}
