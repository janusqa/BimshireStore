namespace BimshireStore.Utility
{

    public static class SD
    {
        public enum ContentType
        {
            Json,
            MultiPartFormData
        }

        public enum ApiMethod
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public static string? CouponApiBaseAddress { get; set; }
    }
}