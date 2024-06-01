
namespace BimshireStore.Services.EmailAPI.Models.Dto
{
    public record EmailLogDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public string? Message { get; set; }
        public DateTime? EmailSent { get; set; }
    }
}