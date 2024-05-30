using BimshireStore.Services.ShoppingCartAPI.Models.Dto;

namespace BimshireStore.Services.ShoppingCartAPI.Services.IService
{
    public interface IHttpRequestMessageBuilder
    {
        HttpRequestMessage Build(ApiRequest apiRequest);
    }
}