
using BimshireStore.Services.OrderAPI.Models.Dto;

namespace BimshireStore.Services.OrderAPI.Services.IService
{
    public interface IBaseService
    {
        Task<ApiResponse?> SendAsync(ApiRequest request, bool withCredentials = true);
    }
}