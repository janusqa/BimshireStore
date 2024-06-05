using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using BimshireStore.Utility;

namespace BimshireStore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _bs;

        public OrderService(IBaseService bs)
        {
            _bs = bs;
        }
        public async Task<ApiResponse?> CreateOrderAsync(CartDto cart)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = cart,
                Url = $"{SD.OrderApiBaseAddress}api/orders"
            });
        }

        public async Task<ApiResponse?> CreateStripeSessionAsync(StripeRequest stripeRequest)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = stripeRequest,
                Url = $"{SD.OrderApiBaseAddress}api/orders/create-stripe-session"
            });
        }

        public async Task<ApiResponse?> GetAllAsync()
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.OrderApiBaseAddress}api/orders"
            });
        }

        public async Task<ApiResponse?> GetByIdAsync(int orderId)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.OrderApiBaseAddress}api/orders/{orderId}"
            });
        }

        public async Task<ApiResponse?> SetStatusAsync(int orderId, string newStatus)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.OrderApiBaseAddress}api/orders/{orderId}/set-status/{newStatus}",
            });
        }

        public async Task<ApiResponse?> ValidateStripeSessionAsync(int orderHeaderId)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = orderHeaderId,
                Url = $"{SD.OrderApiBaseAddress}api/orders/validate-stripe-session"
            });
        }
    }
}