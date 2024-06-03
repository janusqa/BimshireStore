using System.ComponentModel.DataAnnotations;

namespace BimshireStore.Services.RewardAPI.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }
        public required string UserId { get; set; }
        public DateTime RewardDate { get; set; }
        public int OrderId { get; set; }
    }
}