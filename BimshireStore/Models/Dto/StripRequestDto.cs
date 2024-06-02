namespace BimshireStore.Models.Dto
{
    public record StripeRequest
    {
        public required string StripeSessionUrl { get; set; }
        public required string StripeSessionId { get; set; }
        public required string ApprovedUrl { get; set; }
        public required string CancelledUrl { get; set; }
        public required OrderHeaderDto OrderHeaderDto { get; set; }
    }
}
