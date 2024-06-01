using System.Text.Json;

namespace BimshireStore.Services.ShoppingCartAPI.Utility
{

    public static class SD
    {
        public static class JsonSerializerConfig
        {
            public static readonly JsonSerializerOptions DefaultOptions = new() { PropertyNameCaseInsensitive = true };
        }
    }
}