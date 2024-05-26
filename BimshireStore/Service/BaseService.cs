using System.Net;
using System.Text.Json;
using BimshireStore.Models.Dto;
using BimshireStore.Service.IService;
using static BimshireStore.Utility.SD;

namespace BimshireStore.Service
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _hcf;

        public BaseService(IHttpClientFactory hcf)
        {
            _hcf = hcf;
        }

        public async Task<ApiResponse?> SendAsync(ApiRequest request)
        {
            HttpClient client = _hcf.CreateClient("BimshireStoreApi");
            HttpRequestMessage message = new();
            message.Headers.Add("Accept", "application/json");
            message.Method = request.ApiMethod switch
            {
                ApiMethod.POST => HttpMethod.Post,
                ApiMethod.PUT => HttpMethod.Put,
                ApiMethod.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get
            };
            message.RequestUri = new Uri(request.Url);
            // TODO: Add jwt to message header
            if (request.Data is not null) message.Content = new StringContent(JsonSerializer.Serialize(request.Data));

            HttpResponseMessage? response = null;
            response = await client.SendAsync(message);

            try
            {
                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => new ApiResponse { IsSuccess = false, ErrorMessages = ["Not Found"], StatusCode = HttpStatusCode.NotFound },
                    HttpStatusCode.Forbidden => new ApiResponse { IsSuccess = false, ErrorMessages = ["Access Denied"], StatusCode = HttpStatusCode.Forbidden },
                    HttpStatusCode.Unauthorized => new ApiResponse { IsSuccess = false, ErrorMessages = ["Unauthorized"], StatusCode = HttpStatusCode.Unauthorized },
                    HttpStatusCode.InternalServerError => new ApiResponse { IsSuccess = false, ErrorMessages = ["Internal Server Error"], StatusCode = HttpStatusCode.InternalServerError },
                    _ => JsonSerializer.Deserialize<ApiResponse>(await response.Content.ReadAsStringAsync())
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse { IsSuccess = false, ErrorMessages = [ex.Message], StatusCode = HttpStatusCode.InternalServerError };
            }

        }
    }
}