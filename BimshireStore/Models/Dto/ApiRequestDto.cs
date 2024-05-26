using System.Net;

namespace BimshireStore.Models.Dto
{
    public record ApiRequest
    {
        public string ApiType { get; set; } = "GET";
        public string? Url { get; set; }
        public string? AccessToken { get; set; }
        public object? Data { get; set; }
    }
}