using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{
    public interface IOrderService
    {
        Task<ApiResponse?> CreateOrderAsync(CartDto cart);
        Task<ApiResponse?> CreateStripeSessionAsync(StripeRequest stripeRequest);
        Task<ApiResponse?> ValidateStripeSessionAsync(int orderHeaderId);
        Task<ApiResponse?> GetAllAsync();
        Task<ApiResponse?> GetByIdAsync(int orderId);
        Task<ApiResponse?> SetStatusAsync(int orderId, string newStatus);

    }
}