using System.Net;
using System.Text.Json;
using BimshireStore.Services.OrderAPI.Models.Dto;
using BimshireStore.Services.OrderAPI.Services.IService;
using static BimshireStore.Services.OrderAPI.Utility.SD;

namespace BimshireStore.Services.OrderAPI.Services
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

        public async Task<ApiResponse?> SendAsync(ApiRequest request, bool withCredentials)
        {
            HttpClient client = _hcf.CreateClient("BimshireStore");

            // Note for inter api comunications with token we are using DelegatingHandler
            // It is why we do not need to add token here manually like in UI.

            var messageFactory = () => _mb.Build(request);

            try
            {
                HttpResponseMessage? responseMessage = await client.SendAsync(messageFactory());

                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Not Found"], StatusCode = responseMessage.StatusCode };
                    case HttpStatusCode.Forbidden:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Access Denied"], StatusCode = responseMessage.StatusCode };
                    case HttpStatusCode.Unauthorized:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Unauthorized"], StatusCode = responseMessage.StatusCode };
                    case HttpStatusCode.InternalServerError:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Internal Server Error"], StatusCode = responseMessage.StatusCode };
                    default:
                        var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, JsonSerializerConfig.DefaultOptions);
                        return apiResponse;
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