namespace BimshireStore.Services.OrderAPI.Models.Dto
{
    public record RewardDto
    {
        public required string UserId { get; set; }
        public int RewardActivity { get; set; }
        public int OrderId { get; set; }
    }
}