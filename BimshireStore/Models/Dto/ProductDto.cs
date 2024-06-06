using System.ComponentModel.DataAnnotations;

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
        public string? ImageLocalPath { get; set; }
        public IFormFile? File { get; set; }
        [Range(1, 100)]
        public int Count { get; set; } = 1;
    }
}