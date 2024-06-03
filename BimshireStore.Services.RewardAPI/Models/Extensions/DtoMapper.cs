
using BimshireStore.Services.RewardAPI.Models.Dto;

namespace BimshireStore.Services.RewardAPI.Models.Extensions
{
    public static class DtoMapper
    {
        // NB this is an "extension method" for model
        // the "this" keyword allows this to appear as a member method
        // of the model. It allows us to call it like myModel.ToDto
        // which looks much better than DomainExtension.ToDto(myModel).
        // aka it is syntactic sugar over the static method.

        public static RewardDto ToDto(this Reward reward)
        {
            return new RewardDto
            {
                UserId = reward.UserId,
                OrderId = reward.OrderId,
            };
        }
    }
}