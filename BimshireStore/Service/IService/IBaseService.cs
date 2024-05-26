using BimshireStore.Models.Dto;

namespace BimshireStore.Service.IService
{
    public interface IBaseService
    {
        Task<ApiResponse?> SendAsync(ApiRequest request);
    }
}