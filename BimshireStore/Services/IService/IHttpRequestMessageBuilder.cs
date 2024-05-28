using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{
    public interface IHttpRequestMessageBuilder
    {
        HttpRequestMessage Build(ApiRequest apiRequest);
    }
}