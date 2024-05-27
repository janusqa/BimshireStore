using System.Net;
using System.Text.Json;
using BimshireStore.Models.Dto;
using BimshireStore.Service.IService;
using BimshireStore.Utility;

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

            try
            {
                HttpResponseMessage? responseMessage = await client.SendAsync(messageFactory());

                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    var errorMessage = responseMessage.StatusCode switch
                    {
                        HttpStatusCode.NotFound => "Not Found",
                        HttpStatusCode.BadRequest => "Bad Request",
                        HttpStatusCode.Forbidden => "Access Denied",
                        HttpStatusCode.Unauthorized => "Unauthorized",
                        HttpStatusCode.InternalServerError => "Internal Server Error",
                        _ => "Oops, something went wrong. Please try again later"
                    };

                    throw new Exception(errorMessage);
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, JsonSerializerConfig.DefaultOptions);

                if (apiResponse is not null)
                {
                    apiResponse.StatusCode = responseMessage.StatusCode;
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