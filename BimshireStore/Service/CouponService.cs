using BimshireStore.Models.Dto;
using BimshireStore.Service.IService;
using BimshireStore.Utility;

namespace BimshireStore.Service
{
    public class CouponService : BaseService, ICouponService
    {
        public CouponService(IHttpClientFactory hcf, IHttpRequestMessageBuilder mb) : base(hcf, mb)
        {
        }

        public async Task<ApiResponse?> GetAllCouponsAsync()
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons"
            });
        }

        public async Task<ApiResponse?> GetCouponByCodeAsync(string code)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/code/{code}"
            });
        }

        public async Task<ApiResponse?> GetCouponByIdAsync(int id)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/{id}"
            });
        }

        public async Task<ApiResponse?> CreateCouponAsync(CouponDto coupon)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.CouponApiBaseAddress}api/coupons",
                Data = coupon
            });
        }

        public async Task<ApiResponse?> UpdateCouponAsync(CouponDto coupon)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.PUT,
                Url = $"{SD.CouponApiBaseAddress}api/coupons",
                Data = coupon
            });
        }

        public async Task<ApiResponse?> DeleteCouponAsync(int id)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.DELETE,
                Url = $"{SD.CouponApiBaseAddress}api/coupons/{id}"
            });
        }
    }
}