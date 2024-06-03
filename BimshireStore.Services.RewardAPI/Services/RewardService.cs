
using BimshireStore.Services.RewardAPI.Data;
using BimshireStore.Services.RewardAPI.Models;
using BimshireStore.Services.RewardAPI.Models.Dto;
using BimshireStore.Services.RewardAPI.Services.IService;

namespace BimshireStore.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private readonly ApplicationDbContext _db;

        public RewardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> RewardLog(RewardDto rewardDto)
        {
            try
            {
                var reward = new Reward
                {
                    OrderId = rewardDto.OrderId,
                    RewardActivity = rewardDto.RewardActivity,
                    UserId = rewardDto.UserId,
                    RewardDate = rewardDto.RewardDate
                };

                await _db.Rewards.AddAsync(reward);
                await _db.SaveChangesAsync();

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}