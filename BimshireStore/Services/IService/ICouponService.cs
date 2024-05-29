using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{
    public interface ICouponService
    {
        Task<ApiResponse?> GetAllCouponsAsync();
        Task<ApiResponse?> GetCouponByCodeAsync(string code);
        Task<ApiResponse?> GetCouponByIdAsync(int id);
        Task<ApiResponse?> CreateCouponAsync(CouponDto coupon);
        Task<ApiResponse?> UpdateCouponAsync(CouponDto coupon);
        Task<ApiResponse?> DeleteCouponAsync(int id);
    }
}