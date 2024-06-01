using System.ComponentModel.DataAnnotations;

namespace BimshireStore.Services.EmailAPI.Models
{
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }
        public required string Email { get; set; }
        public string? Message { get; set; }
        public DateTime? EmailSent { get; set; }
    }
}