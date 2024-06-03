
using BimshireStore.Services.RewardAPI.Models.Dto;

namespace BimshireStore.Services.RewardAPI.Services.IService
{
    public interface IRewardService
    {
        Task<string> RewardLog(RewardDto reward);
    }
}