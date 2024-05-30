namespace BimshireStore.Models.Dto
{
    public record CartHeaderDto
    {
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponCode { get; set; }
        public double Discount { get; set; }
        public double CartTotal { get; set; }
    }

    public record CartDetailDto
    {
        public int CartDetailId { get; set; }
        public int CartHeaderId { get; set; }
        public CartHeaderDto? CartHeader { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product { get; set; }
        public int Count { get; set; }
    }

    public record CartDto
    {
        public required CartHeaderDto CartHeader { get; set; }
        public IEnumerable<CartDetailDto>? CartDetail { get; set; }
    }
}