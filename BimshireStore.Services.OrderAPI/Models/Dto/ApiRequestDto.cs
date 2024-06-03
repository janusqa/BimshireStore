
using BimshireStore.Services.OrderAPI.Utility;

namespace BimshireStore.Services.OrderAPI.Models.Dto
{
    public record ApiRequest
    {
        public SD.ApiMethod ApiMethod { get; set; } = SD.ApiMethod.GET;
        public string Url { get; set; } = string.Empty;
        public object? Data { get; set; }
        public SD.ContentType ContentType { get; set; } = SD.ContentType.Json;
        public bool WithCredentials { get; set; } = false;
        public string? AccessToken { get; set; } = null;
    }
}