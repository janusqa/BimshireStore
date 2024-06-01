using BimshireStore.Services.EmailAPI.Models.Dto;

namespace BimshireStore.Services.EmailAPI.Services.IService
{
    public interface IEmailService
    {
        Task CartEmailAndLog(CartDto cart);
        Task RegisteredUserEmailAndLog(string email);
        Task OrderPlacedEmailAndLog(RewardDto reward);
    }
}