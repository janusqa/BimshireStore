using BimshireStore.Services.OrderAPI.Models.Dto;

namespace BimshireStore.Services.OrderAPI.Services.IService
{
    public interface IProductService
    {
        Task<ApiResponse?> GetAllAsync();
    }
}