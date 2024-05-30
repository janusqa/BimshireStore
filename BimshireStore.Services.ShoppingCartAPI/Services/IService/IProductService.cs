using BimshireStore.Services.ShoppingCartAPI.Models.Dto;

namespace BimshireStore.Services.ShoppingCartAPI.Services.IService
{
    public interface IProductService
    {
        Task<ApiResponse?> GetAllAsync();
    }
}