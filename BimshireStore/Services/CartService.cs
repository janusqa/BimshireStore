using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using BimshireStore.Utility;

namespace BimshireStore.Services
{

    public class CartService : ICartService
    {
        private readonly IBaseService _bs;

        public CartService(IBaseService bs)
        {
            _bs = bs;
        }

        public async Task<ApiResponse?> ApplyCouponAsync(CartDto cart)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = cart,
                Url = $"{SD.ShoppingCartApiBaseAddress}api/carts/apply-coupon"
            });
        }

        public async Task<ApiResponse?> GetByUserIdAsync(string userId)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.ShoppingCartApiBaseAddress}api/carts/{userId}",
            });
        }

        public async Task<ApiResponse?> RemoveItemAsync(int cartDetailId)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = cartDetailId,
                Url = $"{SD.ShoppingCartApiBaseAddress}api/carts/remove-item"
            });
        }

        public async Task<ApiResponse?> UpsertItemAsync(CartDto cart)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Data = cart,
                Url = $"{SD.ShoppingCartApiBaseAddress}api/carts/upsert-item"
            });
        }
    }
}