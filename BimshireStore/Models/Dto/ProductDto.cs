namespace BimshireStore.Models.Dto
{

    public record ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
    }
}