namespace BimshireStore.Services.OrderAPI.Models.Dto
{
    public record OrderDetailDto
    {
        public int OrderDetailId { get; set; }
        public int OrderHeaderId { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Count { get; set; }
    }
}