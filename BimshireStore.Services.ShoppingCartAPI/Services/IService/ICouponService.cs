using BimshireStore.Services.ShoppingCartAPI.Models.Dto;

namespace BimshireStore.Services.ShoppingCartAPI.Services.IService
{
    public interface ICouponService
    {
        Task<ApiResponse?> GetAllAsync();
        Task<ApiResponse?> GetByCodeAsync(string code);
        Task<ApiResponse?> GetByIdAsync(int id);
        Task<ApiResponse?> CreateAsync(CouponDto coupon);
        Task<ApiResponse?> UpdateAsync(CouponDto coupon);
        Task<ApiResponse?> DeleteAsync(int id);
    }
}