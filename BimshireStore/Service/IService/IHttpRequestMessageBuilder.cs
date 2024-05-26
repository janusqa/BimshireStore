using BimshireStore.Models.Dto;

namespace BimshireStore.Service.IService
{
    public interface IHttpRequestMessageBuilder
    {
        HttpRequestMessage Build(ApiRequest apiRequest);
    }
}