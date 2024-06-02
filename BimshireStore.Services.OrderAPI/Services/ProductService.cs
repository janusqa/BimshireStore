
using BimshireStore.Services.OrderAPI.Models.Dto;
using BimshireStore.Services.OrderAPI.Services.IService;
using BimshireStore.Services.OrderAPI.Utility;

namespace BimshireStore.Services.OrderAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _bs;

        public ProductService(IBaseService bs)
        {
            _bs = bs;
        }

        public async Task<ApiResponse?> GetAllAsync()
        {
            return await _bs.SendAsync(new ApiRequest
            {
                ApiMethod = SD.ApiMethod.GET,
                Url = $"{SD.ProductApiBaseAddress}api/products"
            });
        }
    }
}