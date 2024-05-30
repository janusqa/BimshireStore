
using BimshireStore.Services.ShoppingCartAPI.Models.Dto;

namespace BimshireStore.Services.ShoppingCartAPI.Services.IService
{
    public interface IBaseService
    {
        Task<ApiResponse?> SendAsync(ApiRequest request, bool withCredentials = true);
    }
}