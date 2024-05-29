using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{
    public interface IProductService
    {
        Task<ApiResponse?> GetAllAsync();
        Task<ApiResponse?> GetByIdAsync(int id);
        Task<ApiResponse?> CreateAsync(ProductDto product);
        Task<ApiResponse?> UpdateAsync(ProductDto product);
        Task<ApiResponse?> DeleteAsync(int id);
    }
}