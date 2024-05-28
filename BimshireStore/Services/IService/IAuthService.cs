using BimshireStore.Models.Dto;

namespace BimshireStore.Services.IService
{
    public interface IAuthService
    {
        Task<ApiResponse?> LoginAsync(UserLoginRequestDto request);
        Task<ApiResponse?> RegisterAsync(UserRegisterRequestDto request);
        Task<ApiResponse?> AssignRoleAsync(UserRegisterRequestDto request);
    }

}