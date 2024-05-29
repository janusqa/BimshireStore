using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using BimshireStore.Utility;

namespace BimshireStore.Services
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _bs;

        public AuthService(IBaseService bs)
        {
            _bs = bs;
        }

        public async Task<ApiResponse?> AssignRoleAsync(UserRegisterRequestDto request)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.AuthApiBaseAddress}api/auth/assign-role",
                Data = request
            });
        }

        public async Task<ApiResponse?> LoginAsync(UserLoginRequestDto request)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.AuthApiBaseAddress}api/auth/login",
                Data = request
            });
        }

        public async Task<ApiResponse?> RegisterAsync(UserRegisterRequestDto request)
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.POST,
                Url = $"{SD.AuthApiBaseAddress}api/auth/register",
                Data = request
            });
        }
    }
}