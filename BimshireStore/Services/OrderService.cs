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
        public async Task<ApiResponse?> CreateOrder(CartDto cart)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = cart,
                Url = $"{SD.OrderApiBaseAddress}api/orders"
            });
        }

        public async Task<ApiResponse?> CreateStripeSession(StripeRequest stripeRequest)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = stripeRequest,
                Url = $"{SD.OrderApiBaseAddress}api/orders/create-stripe-session"
            });
        }
    }
}