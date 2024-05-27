using BimshireStore.Services.AuthAPI.Models.Dto;

namespace BimshireStore.Services.AuthAPI.Services.IService
{

    public interface IAuthService
    {
        Task<string> Register(UserRegisterRequestDto request);
        Task<UserAuthResponseDto> Login(UserLoginRequestDto request);
        Task<bool> AssignRole(string userName, string role);

    }
}