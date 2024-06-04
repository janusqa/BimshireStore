using BimshireStore.Services.EmailAPI.Models.Dto;

namespace BimshireStore.Services.EmailAPI.Services.IService
{
    public interface IEmailService
    {
        Task<string> CartEmailAndLog(CartDto cart);
        Task<string> RegisteredUserEmailAndLog(string email);
        Task<string> OrderPlacedEmailAndLog(RewardDto reward);
    }
}