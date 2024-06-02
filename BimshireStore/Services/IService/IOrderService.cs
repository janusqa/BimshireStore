using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{
    public interface IOrderService
    {
        Task<ApiResponse?> CreateOrder(CartDto cart);
        Task<ApiResponse?> CreateStripeSession(StripeRequest stripeRequest);
    }
}