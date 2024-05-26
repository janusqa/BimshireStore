using System.Net;
using System.Text.Json;
using BimshireStore.Models.Dto;
using BimshireStore.Service.IService;
using BimshireStore.Utility;
using static BimshireStore.Utility.SD;

namespace BimshireStore.Service
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _hcf;
        private readonly IHttpRequestMessageBuilder _mb;


        public BaseService(IHttpClientFactory hcf, IHttpRequestMessageBuilder mb)
        {
            _hcf = hcf;
            _mb = mb;
        }

        public async Task<ApiResponse?> SendAsync(ApiRequest request)
        {
            HttpClient client = _hcf.CreateClient("BimshireStoreApi");

            var messageFactory = () => _mb.Build(request);

            HttpResponseMessage? responseMessage = await client.SendAsync(messageFactory());

            try
            {
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(jsonResponse)) throw new Exception("Empty response from service");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, JsonSerializerConfig.DefaultOptions);

                if (!responseMessage.IsSuccessStatusCode && apiResponse is not null)
                {
                    apiResponse.ErrorMessages = responseMessage.StatusCode switch
                    {
                        HttpStatusCode.NotFound => [.. (apiResponse.ErrorMessages ?? []), "Not Found"],
                        HttpStatusCode.Forbidden => [.. (apiResponse.ErrorMessages ?? []), "Access Denied"],
                        HttpStatusCode.Unauthorized => [.. (apiResponse.ErrorMessages ?? []), "Unauthorized"],
                        HttpStatusCode.InternalServerError => [.. (apiResponse.ErrorMessages ?? []), "Internal Server Error"],
                        _ => [.. (apiResponse.ErrorMessages ?? []), "Oops, something went wrong. Please try again later"]
                    };
                }

                if (apiResponse is not null)
                {
                    apiResponse.StatusCode = responseMessage.StatusCode;
                    // var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(apiResponse));
                    return apiResponse;
                }
                else
                {
                    throw new Exception("Oops something went wrong");
                }

            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    ErrorMessages = ex.InnerException?.Message is not null ? [ex.Message, ex.InnerException.Message] : [ex.Message],
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }

        }
    }
}