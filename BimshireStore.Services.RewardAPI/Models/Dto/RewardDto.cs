namespace BimshireStore.Services.RewardAPI.Models.Dto
{
    public record RewardDto
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int OrderId { get; set; }
        public int RewardActivity { get; set; }
        public DateTime RewardDate { get; set; }
    }
}