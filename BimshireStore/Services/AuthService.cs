using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using BimshireStore.Utility;

namespace BimshireStore.Services
{
    public class AuthService : BaseService, IAuthService
    {
        public AuthService(IHttpClientFactory hcf, IHttpRequestMessageBuilder mb) : base(hcf, mb)
        {

        }

        public async Task<ApiResponse?> AssignRoleAsync(UserRegisterRequestDto request)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.AuthApiBaseAddress}api/auth/assign-role",
                Data = request
            });
        }

        public async Task<ApiResponse?> LoginAsync(UserLoginRequestDto request)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.AuthApiBaseAddress}api/auth/login",
                Data = request
            });
        }

        public async Task<ApiResponse?> RegisterAsync(UserRegisterRequestDto request)
        {
            return await SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.AuthApiBaseAddress}api/auth/register",
                Data = request
            });
        }
    }
}