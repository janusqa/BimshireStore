using static BimshireStore.Services.OrderAPI.Utility.SD;

namespace BimshireStore.Services.OrderAPI.Models.Dto
{
    public record ApiRequest
    {
        public ApiMethod ApiMethod { get; set; } = ApiMethod.GET;
        public string Url { get; set; } = string.Empty;
        public object? Data { get; set; }
        public ContentType ContentType { get; set; } = ContentType.Json;
        public bool WithCredentials { get; set; } = false;
        public string? AccessToken { get; set; } = null;
    }
}