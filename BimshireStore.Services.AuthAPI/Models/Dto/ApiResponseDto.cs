using System.Net;

namespace BimshireStore.Services.AuthAPI.Models.Dto
{
    public record ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public object? Result { get; set; }
    }
}