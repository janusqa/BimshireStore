using BimshireStore.Services.ShoppingCartAPI.Models.Dto;
using BimshireStore.Services.ShoppingCartAPI.Services.IService;
using BimshireStore.Services.ShoppingCartAPI.Utility;

namespace BimshireStore.Services.ShoppingCartAPI.Services
{
    public class CouponService : ICouponService
    {
        private readonly IBaseService _bs;

        public CouponService(IBaseService bs)
        {
            _bs = bs;
        }

        public async Task<ApiResponse?> GetAllAsync()
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons"
            });
        }

        public async Task<ApiResponse?> GetByCodeAsync(string code)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/code/{code}"
            });
        }

        public async Task<ApiResponse?> GetByIdAsync(int id)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/{id}"
            });
        }

        public async Task<ApiResponse?> CreateAsync(CouponDto coupon)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.CouponApiBaseAddress}api/coupons",
                Data = coupon
            });
        }

        public async Task<ApiResponse?> UpdateAsync(CouponDto coupon)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.PUT,
                Url = $"{SD.CouponApiBaseAddress}api/coupons",
                Data = coupon
            });
        }

        public async Task<ApiResponse?> DeleteAsync(int id)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.DELETE,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/{id}"
            });
        }

    }
}