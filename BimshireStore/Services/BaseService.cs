using System.Net;
using System.Text.Json;
using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using static BimshireStore.Utility.SD;

namespace BimshireStore.Services
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

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, JsonSerializerConfig.DefaultOptions);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    var errorMessage = responseMessage.StatusCode switch
                    {
                        HttpStatusCode.NotFound => string.Join(",", apiResponse?.ErrorMessages ?? ["Not Found"]),
                        HttpStatusCode.BadRequest => string.Join(",", apiResponse?.ErrorMessages ?? ["Bad Request"]),
                        HttpStatusCode.Forbidden => string.Join(",", apiResponse?.ErrorMessages ?? ["Access denied"]),
                        HttpStatusCode.Unauthorized => string.Join(",", apiResponse?.ErrorMessages ?? ["Unauthorized"]),
                        HttpStatusCode.InternalServerError => string.Join(",", apiResponse?.ErrorMessages ?? ["Internal Server Error"]),
                        _ => string.Join(",", apiResponse?.ErrorMessages ?? ["Oops, something went wrong"])
                    };

                    throw new Exception(errorMessage);
                }

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