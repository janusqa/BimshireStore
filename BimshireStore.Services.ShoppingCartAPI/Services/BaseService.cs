using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BimshireStore.Services.ShoppingCartAPI.Models.Dto;
using BimshireStore.Services.ShoppingCartAPI.Services.IService;
using static BimshireStore.Services.ShoppingCartAPI.Utility.SD;

namespace BimshireStore.Services.ShoppingCartAPI.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _hcf;
        private readonly IHttpRequestMessageBuilder _mb;
        private readonly ITokenService _ts;

        public BaseService(IHttpClientFactory hcf, IHttpRequestMessageBuilder mb, ITokenService ts)
        {
            _hcf = hcf;
            _mb = mb;
            _ts = ts;
        }

        public async Task<ApiResponse?> SendAsync(ApiRequest request, bool withCredentials)
        {
            HttpClient client = _hcf.CreateClient("BimshireStore");

            if (withCredentials)
            {
                var token = _ts.GetToken();
                if (token is not null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            var messageFactory = () => _mb.Build(request);

            try
            {
                HttpResponseMessage? responseMessage = await client.SendAsync(messageFactory());

                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Not Found"] };
                    case HttpStatusCode.Forbidden:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Access Denied"] };
                    case HttpStatusCode.Unauthorized:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Unauthorized"] };
                    case HttpStatusCode.InternalServerError:
                        return new ApiResponse { IsSuccess = false, ErrorMessages = ["Internal Server Error"] };
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