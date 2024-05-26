namespace BimshireStore.Models.Dto
{

    public record CouponDto
    {
        public int CouponId { get; set; }
        public required string CouponCode { get; set; }
        public double DiscountAmount { get; set; }
        public int MinAmount { get; set; }
    }

}