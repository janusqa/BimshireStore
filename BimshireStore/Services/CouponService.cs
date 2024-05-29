using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using BimshireStore.Utility;

namespace BimshireStore.Services
{
    public class CouponService : ICouponService
    {
        private readonly IBaseService _bs;

        public CouponService(IBaseService bs)
        {
            _bs = bs;
        }

        public async Task<ApiResponse?> GetAllCouponsAsync()
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons"
            });
        }

        public async Task<ApiResponse?> GetCouponByCodeAsync(string code)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/code/{code}"
            });
        }

        public async Task<ApiResponse?> GetCouponByIdAsync(int id)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/{id}"
            });
        }

        public async Task<ApiResponse?> CreateCouponAsync(CouponDto coupon)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.CouponApiBaseAddress}api/coupons",
                Data = coupon
            });
        }

        public async Task<ApiResponse?> UpdateCouponAsync(CouponDto coupon)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.PUT,
                Url = $"{SD.CouponApiBaseAddress}api/coupons",
                Data = coupon
            });
        }

        public async Task<ApiResponse?> DeleteCouponAsync(int id)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.DELETE,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/{id}"
            });
        }

    }
}