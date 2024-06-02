using System.ComponentModel.DataAnnotations;

namespace BimshireStore.Services.OrderAPI.Models.Dto
{
    public record OrderHeaderDto
    {
        [Key]
        public int OrderHeaderId { get; set; }
        public string? UserId { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Phone { get; set; }
        [Required]
        public string? Email { get; set; }
        public string? CouponCode { get; set; }
        public double Discount { get; set; }
        public double OrderTotal { get; set; }
        public DateTime OrderTime { get; set; }
        public string? Status { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? StripeSessionId { get; set; }
        public IEnumerable<OrderDetailDto> OrderDetails { get; set; } = [];
    }
}