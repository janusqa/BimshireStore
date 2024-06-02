using BimshireStore.Services.OrderAPI.Models.Dto;

namespace BimshireStore.Services.OrderAPI.Services.IService
{
    public interface IHttpRequestMessageBuilder
    {
        HttpRequestMessage Build(ApiRequest apiRequest);
    }
}