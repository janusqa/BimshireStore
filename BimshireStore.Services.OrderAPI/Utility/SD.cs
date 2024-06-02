using System.Text.Json;

namespace BimshireStore.Services.OrderAPI.Utility
{

    public static class SD
    {
        // Role Constants
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        public static string? ProductApiBaseAddress { get; set; }

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

        public const string TokenCookie = "BimshireStoreJwtToken";

        public static class JsonSerializerConfig
        {
            public static readonly JsonSerializerOptions DefaultOptions = new() { PropertyNameCaseInsensitive = true };
        }

        // Order Statuses
        public const string Status_Pending = "Pending";
        public const string Status_Approved = "Approved";
        public const string Status_ReadyForPickup = "ReadyForPickup";
        public const string Status_Completed = "Completed";
        public const string Status_Refunded = "Refunded";
        public const string Status_Cancelled = "Cancelled";
    }
}