namespace BimshireStore.Services.EmailAPI.Models.Dto
{
    public record RewardDto
    {
        public required string UserId { get; set; }
        public int RewardsActivity { get; set; }
        public int OrderId { get; set; }
    }
}