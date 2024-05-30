using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{

    public interface ICartService
    {
        Task<ApiResponse?> GetByUserIdAsync(string userId);
        Task<ApiResponse?> UpsertItemAsync(CartDto cart);
        Task<ApiResponse?> RemoveItemAsync(int cartDetailId);
        Task<ApiResponse?> ApplyCouponAsync(CartDto cart);
    }
}