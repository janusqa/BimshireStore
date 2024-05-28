using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{
    public interface IBaseService
    {
        Task<ApiResponse?> SendAsync(ApiRequest request);
    }
}